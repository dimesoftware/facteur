using System;
using System.Threading.Tasks;
using Facteur.Smtp;
using Facteur.TemplateProviders.IO;
using Facteur.Tests;
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
                    x.WithMailer(x => new SmtpMailer(credentials))
                    .WithCompiler<ScribanCompiler>()
                    .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                    .WithResolver<ViewModelTemplateResolver>()
                    .WithTemplatedComposer();
                });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            IEmailComposer<TestMailModel> composer = serviceProvider.GetService<IEmailComposer<TestMailModel>>();
            EmailRequest request = await composer
                .SetModel(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" })
                .SetSubject("Hello world")
                .SetFrom("info@facteur.com")
                .SetTo("byziji2958@chapsmail.com")
                .BuildAsync();

            IMailer mailer = serviceProvider.GetService<IMailer>();
            //await mailer.SendMailAsync(request);
        }
    }
}