using System.Threading.Tasks;
using Facteur.Smtp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    public class SmtpTests
    {
        [TestMethod]
        public async Task Smtp_SendTemplateMail_ShouldSend()
        {
            SmtpCredentials credentials = new SmtpCredentials("smtp.mailtrap.io", "587", "false", "true", "d3538ae47a016d", "d4add3690c408c");

            EmailComposer<TestMailModel> composer = new EmailComposer<TestMailModel>();
            EmailRequest<TestMailModel> request = composer
                .SetModel(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" })
                .SetBody("<html><body><h1>Hello world!</h1></body></html>")
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("tibipi@getnada.com")
                .SetCc("tibipi@getnada.com")
                .SetBcc("tibipi@getnada.com")
                .Build();

            IMailer mailer = new SmtpMailer(credentials, new ViewModelTemplateResolver(), new SimpleHtmlTemplateCompiler());
            await mailer.SendMailAsync(request);
        }
    }
}