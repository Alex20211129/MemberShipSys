using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MemberShipSys.Services
{
    public class ResendEmailSender : IEmailSender 
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ResendEmailSender(IHttpClientFactory httpClientFactory , IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlbody)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            request.Headers.Authorization =new AuthenticationHeaderValue("Bearer", _configuration["Resend:ApiKey"]);
            request.Content = JsonContent.Create(new
            {
                from = _configuration["Resend:FromAddress"],
                to = new[] { toEmail},
                subject,
                html = htmlbody
            });

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
