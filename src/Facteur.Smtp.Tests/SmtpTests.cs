using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Facteur.Smtp;
using Facteur.TemplateProviders.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    public class SmtpTests
    {
        [TestMethod]
        public async Task Smtp_SendTemplateMail_ShouldSend()
        {
            SmtpCredentials credentials = new("smtp.mailtrap.io", "587", "false", "true", "d3538ae47a016d", "d4add3690c408c");

            EmailComposer<TestMailModel> composer = new();
            EmailRequest<TestMailModel> request = composer
                .SetModel(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" })
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com", "Facteur")
                .SetTo("tibipi@getnada.com")
                .SetCc("tibipi@getnada.com")
                .SetBcc("tibipi@getnada.com")
                .Build();

            IMailer mailer = new SmtpMailer(credentials);

            IMailBodyBuilder builder = new MailBodyBuilder();
            EmailRequest populatedRequest = await builder
                .UseProvider(new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                .UseResolver(new ViewModelTemplateResolver())
                .UseCompiler(new ScribanCompiler())
                .BuildAsync(request);

            await mailer.SendMailAsync(populatedRequest);
        }

        [TestMethod]
        public async Task Smtp_SendTemplateMail_SimulateDependencyInjection_ShouldSend()
        {
            SmtpCredentials credentials = new("smtp.mailtrap.io", "587", "false", "true", "d3538ae47a016d", "d4add3690c408c");

            EmailComposer<TestMailModel> composer = new();
            EmailRequest<TestMailModel> request = composer
                .SetModel(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" })
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("tibipi@getnada.com")
                .SetCc("tibipi@getnada.com")
                .SetBcc("tibipi@getnada.com")
                .Build();

            IMailer mailer = new SmtpMailer(credentials);

            IMailBodyBuilder builder = new MailBodyBuilder(
                new ScribanCompiler(),
                new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
                new ViewModelTemplateResolver());

            EmailRequest populatedRequest = await builder.BuildAsync(request);
            await mailer.SendMailAsync(populatedRequest);
        }

        [TestMethod]
        public async Task Smtp_SendTemplateMail_WithAttachments_ShouldSend()
        {
            SmtpCredentials credentials = new("smtp.mailtrap.io", "587", "false", "true", "d3538ae47a016d", "d4add3690c408c");

            byte[] txtBytes = File.ReadAllBytes("Attachments\\Attachment.txt");
            byte[] pdfBytes = File.ReadAllBytes("Attachments\\Attachment.pdf");

            List<Attachment> attachments = new()
            {
                new Attachment() { ContentBytes = txtBytes, Name = "Attachment.txt" },
                new Attachment() { ContentBytes = pdfBytes, Name = "Attachment.pdf" }
            };

            EmailComposer<TestMailModel> composer = new();
            EmailRequest<TestMailModel> request = composer
                .SetModel(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" })
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("tibipi@getnada.com")
                .SetCc("tibipi@getnada.com")
                .SetBcc("tibipi@getnada.com")
                .Attach(attachments)
                .Build();

            IMailer mailer = new SmtpMailer(credentials);

            IMailBodyBuilder builder = new MailBodyBuilder();
            EmailRequest populatedRequest = await builder
                .UseProvider(new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                .UseResolver(new ViewModelTemplateResolver())
                .UseCompiler(new ScribanCompiler())
                .BuildAsync(request);

            await mailer.SendMailAsync(populatedRequest);
        }
    }
}