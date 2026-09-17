using System;
using System.Collections.Generic;
using System.Text;
using MemberShipSys.Services.Hashing;
namespace MemberShipSys.Tests
{
    public class Pbkdf2HashStrategyTests
    {
        [Fact]
        public void Hash_ReturnsExpectedHash()
        {
            // Arrange：準備測試需要的東西
            var pbkdf2 = new Pbkdf2HashStrategy();

            // Act：執行你要測的那個動作
            string HashedResult = pbkdf2.Hash("MyPassword123");
            bool VerifiedResult = pbkdf2.Verify("MyPassword123", HashedResult);

            // Assert：斷言結果符合預期，不符合的話這個測試就會顯示失敗
            Assert.NotNull(HashedResult);
            Assert.True(VerifiedResult);
        }

        [Fact]
        public void Hash_ReturnsExpectedWrongPassword()
        {
            // Arrange：準備測試需要的東西
            var pbkdf2 = new Pbkdf2HashStrategy();

            // Act：執行你要測的那個動作
            string HashedResult = pbkdf2.Hash("MyPassword123");
            bool VerifiedResult = pbkdf2.Verify("WrongPassword", HashedResult);

            // Assert：斷言結果符合預期，不符合的話這個測試就會顯示失敗
            Assert.NotNull(HashedResult);
            Assert.False(VerifiedResult);
        }
    }
}
