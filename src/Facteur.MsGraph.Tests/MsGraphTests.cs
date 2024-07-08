using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.MsGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Facteur.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MsGraphTests
    {
        [DataTestMethod]
        [DataRow("", "tenantId", "clientSecret", "from")]
        [DataRow("clientId", "", "clientSecret", "from")]
        [DataRow("clientId", "tenantId", "", "from")]
        [DataRow("clientId", "tenantId", "clientSecret", "")]
        public void Graph_SendMail_InvalidParameter_ShouldThrowArgumentNullException(string clientId, string tenantId, string clientSecret, string from)
            => Assert.ThrowsException<ArgumentNullException>(() => new GraphCredentials(clientId, tenantId, clientSecret, @from));

        [TestMethod]
        public async Task Graph_SendMail_ShouldSend()
        {
            GraphCredentials credentials = new("clientId", "tenantId", "secret", "from");

            EmailComposer composer = new();
            EmailRequest request = await composer
                .Subject("Hello world")
                .From("info@facteur.com")
                .To("tibipi@getnada.com")
                .BuildAsync(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" });

            Mock<IMailer> mock = new();
            mock.Setup(foo => foo.SendMailAsync(request)).Returns(Task.CompletedTask);
        }
    }
}