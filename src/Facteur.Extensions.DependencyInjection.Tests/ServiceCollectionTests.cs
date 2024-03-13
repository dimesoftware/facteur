using System;
using System.Threading.Tasks;
using Facteur.Smtp;
using Facteur.TemplateProviders.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Extensions.DependencyInjection.Tests
{
    [TestClass]
    public class ServiceCollectionTests
    {
        [TestMethod]
        public async Task ServiceCollection_DI_ShouldConstructAndSendMail()
        {
            string testEmail = Environment.GetEnvironmentVariable("TEST_SMTP_EMAIL");
            string testPw = Environment.GetEnvironmentVariable("TEST_SMTP_PASSWORD");

            SmtpCredentials credentials = new("sandbox.smtp.mailtrap.io", "2525", "false", "true", testEmail, testPw);

            ServiceCollection serviceCollection = new();
            serviceCollection
                .AddFacteur(x =>
                {
                    x.WithMailer(y => new SmtpMailer(credentials, y.GetService<IEmailComposer>()))
                    .WithCompiler<ScribanCompiler>()
                    .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                    .WithResolver<ViewModelTemplateResolver>()
                    .WithDefaultComposer();
                });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            IMailer mailer = serviceProvider.GetService<IMailer>();
            //await mailer.SendMailAsync(x => x
            //.SetSubject("Hello world")
            //.SetFrom("info@facteur.com")
            //.SetTo("byziji2958@chapsmail.com")
            //.BuildAsync(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" }));
        }
    }
}