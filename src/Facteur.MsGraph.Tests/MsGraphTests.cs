using System;
using System.Threading.Tasks;
using Facteur.MsGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Facteur.Tests
{
    [TestClass]
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
        public void Graph_SendMail_ShouldSend()
        {
            GraphCredentials credentials = new GraphCredentials("clientId", "tenantId", "secret", "from");

            EmailComposer<TestMailModel> composer = new EmailComposer<TestMailModel>();
            EmailRequest<TestMailModel> request = composer
                .SetModel(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" })
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("tibipi@getnada.com")
                .Build();

            Mock<IMailer> mock = new Mock<IMailer>();
            mock.Setup(foo => foo.SendMailAsync(request)).Returns(Task.CompletedTask);
        }
    }
}