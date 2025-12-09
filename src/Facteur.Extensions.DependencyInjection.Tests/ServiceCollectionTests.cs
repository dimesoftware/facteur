using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.Extensions.DependencyInjection.Resiliency;
using Facteur.Smtp;
using Facteur.TemplateProviders.IO;
using Facteur.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Retry;

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

        [TestMethod]
        public async Task ServiceCollection_WithMailers_FailoverShouldWork()
        {
            // Arrange
            TestMailer failingMailer = new(shouldSucceed: false, id: 1);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 2);

            ServiceCollection serviceCollection = new();
            serviceCollection
                .AddFacteur(x =>
                {
                    x.WithMailers(config =>
                    {
                        config.WithMailer(_ => failingMailer, policy =>
                        {
                            policy.AddRetry(new RetryStrategyOptions
                            {
                                MaxRetryAttempts = 2,
                                ShouldHandle = new PredicateBuilder().Handle<Exception>()
                            });
                        });

                        config.WithMailer(_ => successfulMailer);
                    })
                    .WithCompiler<ScribanCompiler>()
                    .WithTemplateProvider(_ => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                    .WithResolver<ViewModelTemplateResolver>()
                    .WithDefaultComposer();
                });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            IMailer mailer = serviceProvider.GetRequiredService<IMailer>();

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"],
                Body = "Test email body"
            };

            // Act
            await mailer.SendMailAsync(request);

            // Assert
            Assert.AreEqual(3, failingMailer.CallCount, "Failing mailer should have been called 3 times (1 initial + 2 retries)");
            Assert.AreEqual(1, successfulMailer.CallCount, "Successful mailer should have been called once after first mailer failed");
        }

        [TestMethod]
        public async Task ServiceCollection_WithMailers_AllFail_ShouldThrowAggregateException()
        {
            // Arrange
            TestMailer failingMailer1 = new(shouldSucceed: false, id: 1);
            TestMailer failingMailer2 = new(shouldSucceed: false, id: 2);

            ServiceCollection serviceCollection = new();
            serviceCollection
                .AddFacteur(x =>
                {
                    x.WithMailers(config =>
                    {
                        config.WithMailer(_ => failingMailer1, policy =>
                        {
                            policy.AddRetry(new RetryStrategyOptions
                            {
                                MaxRetryAttempts = 1,
                                ShouldHandle = new PredicateBuilder().Handle<Exception>()
                            });
                        });

                        config.WithMailer(_ => failingMailer2);
                    })
                    .WithCompiler<ScribanCompiler>()
                    .WithTemplateProvider(_ => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                    .WithResolver<ViewModelTemplateResolver>()
                    .WithDefaultComposer();
                });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            IMailer mailer = serviceProvider.GetRequiredService<IMailer>();

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"],
                Body = "Test email body"
            };

            // Act & Assert
            AggregateException exception = await Assert.ThrowsAsync<AggregateException>(
                () => mailer.SendMailAsync(request));

            Assert.IsNotNull(exception, "Should throw AggregateException when all mailers fail");
            Assert.IsTrue(exception.InnerExceptions.Count >= 2, "Should have exceptions from all mailer attempts");
            Assert.AreEqual(2, failingMailer1.CallCount, "First mailer should have been called twice (1 initial + 1 retry)");
            Assert.AreEqual(1, failingMailer2.CallCount, "Second mailer should have been called once");
        }

        [TestMethod]
        public async Task ServiceCollection_WithMailers_ComposeEmail_ShouldWork()
        {
            // Arrange
            TestMailer failingMailer = new(shouldSucceed: false, id: 1);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 2);

            ServiceCollection serviceCollection = new();
            serviceCollection
                .AddFacteur(x =>
                {
                    x.WithMailers(config =>
                    {
                        config.WithMailer(_ => failingMailer);
                        config.WithMailer(_ => successfulMailer);
                    })
                    .WithCompiler<ScribanCompiler>()
                    .WithTemplateProvider(_ => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                    .WithResolver<ViewModelTemplateResolver>()
                    .WithDefaultComposer();
                });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            IMailer mailer = serviceProvider.GetRequiredService<IMailer>();

            // Act
            await mailer.SendMailAsync(async composer =>
            {
                return await composer
                    .Subject("Hello world")
                    .From("info@facteur.com", "Facteur")
                    .To("test@example.com")
                    .BuildAsync(new TestMailModel { Email = "guy.gadbois@facteur.com", Name = "Guy Gadbois" });
            });

            // Assert
            Assert.AreEqual(1, failingMailer.CallCount, "Failing mailer should have been called once");
            Assert.AreEqual(1, successfulMailer.CallCount, "Successful mailer should have been called once");
        }

    }
}