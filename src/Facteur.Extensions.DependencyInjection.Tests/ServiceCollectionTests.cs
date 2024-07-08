using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.Smtp;
using Facteur.TemplateProviders.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Extensions.DependencyInjection.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ServiceCollectionTests
    {
        [TestMethod]
        public async Task ServiceCollection_Defaults_ShouldConstructAndSendMail()
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
            //.Subject("Hello world")
            //.From("info@facteur.com")
            //.To("byziji2958@chapsmail.com")
            //.BuildAsync(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" }));
        }

        [TestMethod]
        public async Task ServiceCollection_Composer_Default_ShouldConstructAndSendMail()
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
                    .WithComposer<EmailComposer>();
                });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            IMailer mailer = serviceProvider.GetService<IMailer>();
        }

        [TestMethod]
        public async Task ServiceCollection_Composer_Factory_ShouldConstructAndSendMail()
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
                    .WithComposer(x => new EmailComposer(x.GetService<ITemplateCompiler>(), x.GetService<ITemplateProvider>(), x.GetService<ITemplateResolver>()));
                });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            IMailer mailer = serviceProvider.GetService<IMailer>();
        }

        [TestMethod]
        public async Task ServiceCollection_FacteurCollection_ShouldConstructAndSendMail()
        {
            string testEmail = Environment.GetEnvironmentVariable("TEST_SMTP_EMAIL");
            string testPw = Environment.GetEnvironmentVariable("TEST_SMTP_PASSWORD");

            SmtpCredentials credentials = new("sandbox.smtp.mailtrap.io", "2525", "false", "true", testEmail, testPw);

            ServiceCollection serviceCollection = new();
            serviceCollection
                .AddFacteur()
                .WithMailer(y => new SmtpMailer(credentials, y.GetService<IEmailComposer>()))
                .WithCompiler<ScribanCompiler>()
                .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                .WithResolver<ViewModelTemplateResolver>()
                .WithComposer<EmailComposer>();

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            IMailer mailer = serviceProvider.GetService<IMailer>();
        }

    }
}