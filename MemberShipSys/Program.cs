using MemberShipSys.Data;
using MemberShipSys.Models;
using MemberShipSys.Services;
using MemberShipSys.Services.Hashing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

//註冊DB、Identity
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<MembershipUser, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 3;  //設定登入失敗幾次就鎖帳號
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);  //鎖帳號多久
    options.SignIn.RequireConfirmedAccount = true; //需要驗證帳號
})
    .AddEntityFrameworkStores<ApplicationDbContext>()  //告訴 Identity 要用哪個 DbContext 存資料
    .AddDefaultTokenProviders();  //這是忘記密碼/Email 驗證那些 token（產生、驗證、過期）背後的機制

//AddIdentity 內部已經用 TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>() 註冊了它自己預設的 hasher。
//用一般的 AddScoped（不是 TryAdd）再註冊一次你自己的版本——後註冊的會覆蓋掉前面的
builder.Services.AddScoped<IPasswordHasher<MembershipUser>, MembershipPasswordHasher>();
//註冊Resend寄信功能
builder.Services.AddScoped<IEmailSender, ResendEmailSender>();
//SecurityStamp（一個隨機字串，存在資料庫裡）
//ASP.NET Core 會定期（不是每個請求都查）拿 cookie 裡的值跟資料庫裡目前的值比對，兩者對不上，就會自動把這個人登出。
//這裡設定每 1 分鐘就比對一次，預設是 30 分鐘 (畢竟每次檢查都是一次資料庫查詢，間隔太短等於變相增加資料庫負載）
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(1);
});

builder.Services.AddSingleton<Pbkdf2HashStrategy>();
builder.Services.AddSingleton<BCryptHashStrategy>();
builder.Services.AddSingleton<Argon2HashStrategy>();
builder.Services.AddScoped<ICurrentAlgorithmProvider, DbCurrentAlgorithmProvider>();
var app = builder.Build();

// 這裡面才能安全使用 Scoped 服務
using (var scope = app.Services.CreateScope())
{
    //設定會員角色預設值
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MembershipUser>>();


    string[] role = { "Admin", "Member" };
    foreach (var roleName in role)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    string adminEmail = builder.Configuration["AdminSeed:Email"] ?? "admin@example.com";
    string adminPassword = builder.Configuration["AdminSeed:Password"] ?? "Admin123!";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser is null)
    {
        adminUser = new MembershipUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            throw new Exception("Seed admin user failed: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    //設定雜湊驗算法預設值
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!await dbContext.AppSettings.AnyAsync())
    {
        dbContext.AppSettings.Add(new AppSetting { CurrentHashAlgorithm = PasswordHashAlgorithm.Argon2 });
        await dbContext.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
