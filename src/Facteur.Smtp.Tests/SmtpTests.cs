using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Facteur.Smtp;
using Facteur.TemplateProviders.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using netDumbster.smtp;

namespace Facteur.Tests
{
    [TestClass]
    public class SmtpTests
    {
        [TestMethod]
        public async Task Smtp_SendTemplateMail_ShouldSend()
        {
            SimpleSmtpServer server = SimpleSmtpServer.Start(2525);
            SmtpCredentials credentials = new("localhost", "2525", "false", "false");

            IEmailComposer composer = new EmailComposer(
               new ScribanCompiler(),
               new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
               new ViewModelTemplateResolver());

            EmailRequest request = await composer
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com", "Facteur")
                .SetTo("tibipi@getnada.com")
                .SetCc("tibipi@getnada.com")
                .SetBcc("tibipi@getnada.com")
                .BuildAsync(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" });

            IMailer mailer = new SmtpMailer(credentials);
            await mailer.SendMailAsync(request);
        }

        [TestMethod]
        public async Task Smtp_SendTemplateMail_SimulateDependencyInjection_ShouldSend()
        {
            SimpleSmtpServer server = SimpleSmtpServer.Start(2525);
            SmtpCredentials credentials = new("localhost", "2525", "false", "false");

            EmailComposer composer = new(
                new ScribanCompiler(),
                new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
                new ViewModelTemplateResolver());

            EmailRequest request = await composer                
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("tibipi@getnada.com")
                .SetCc("tibipi@getnada.com")
                .SetBcc("tibipi@getnada.com")
                .BuildAsync(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" });

            IMailer mailer = new SmtpMailer(credentials);

            await mailer.SendMailAsync(request);

            //Thread.Sleep(1000);

            //SmtpMessage smtpMessage = server.ReceivedEmail[0];
            //Assert.IsTrue(smtpMessage.FromAddress.Address == "info@facteur.com");
        }

        [TestMethod]
        public async Task Smtp_SendTemplateMail_WithAttachments_ShouldSend()
        {
            SimpleSmtpServer server = SimpleSmtpServer.Start(2525);
            SmtpCredentials credentials = new("localhost", "2525", "false", "false");

            byte[] txtBytes = File.ReadAllBytes("Attachments/Attachment.txt");
            byte[] pdfBytes = File.ReadAllBytes("Attachments/Attachment.pdf");

            List<Attachment> attachments = new()
            {
                new Attachment() { ContentBytes = txtBytes, Name = "Attachment.txt" },
                new Attachment() { ContentBytes = pdfBytes, Name = "Attachment.pdf" }
            };

            EmailComposer composer = new(
                new ScribanCompiler(),
                new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
                new ViewModelTemplateResolver());

            EmailRequest request = await composer
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("tibipi@getnada.com")
                .SetCc("tibipi@getnada.com")
                .SetBcc("tibipi@getnada.com")
                .Attach(attachments)
                .BuildAsync(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" });

            IMailer mailer = new SmtpMailer(credentials);

            await mailer.SendMailAsync(request);

            //Thread.Sleep(1000);

            //SmtpMessage smtpMessage = server.ReceivedEmail[0];
            //Assert.IsTrue(smtpMessage.FromAddress.Address == "info@facteur.com");
        }
    }
}