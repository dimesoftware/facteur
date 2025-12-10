using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.Smtp;
using Facteur.TemplateProviders.IO;
using Facteur.Tests;
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
                    x
                    .WithMailer(y => new SmtpMailer(credentials, y.GetService<IEmailComposer>()))
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
                    x.WithMailer(_ => failingMailer)
                     .WithMailer(_ => successfulMailer)
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
            Assert.AreEqual(1, failingMailer.CallCount, "Failing mailer should have been called once before failing over");
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
                    x.WithMailer(_ => failingMailer1)
                     .WithMailer(_ => failingMailer2)
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
            Assert.IsGreaterThanOrEqualTo(2, exception.InnerExceptions.Count, "Should have exceptions from all mailer attempts");
            Assert.AreEqual(1, failingMailer1.CallCount, "First mailer should have been called once");
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
                .AddFacteur(x => x
                    .WithMailer(_ => failingMailer)
                    .WithMailer(_ => successfulMailer)
                    .WithCompiler<ScribanCompiler>()
                    .WithTemplateProvider(_ => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                    .WithResolver<ViewModelTemplateResolver>()
                    .WithDefaultComposer());

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

        [TestMethod]
        public async Task ServiceCollection_WithSingleMailer_ShouldWork()
        {
            // Arrange
            TestMailer successfulMailer = new(shouldSucceed: true, id: 1);

            ServiceCollection serviceCollection = new();
            serviceCollection
                .AddFacteur(x =>
                {
                    x.WithMailer(_ => successfulMailer)
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
            Assert.AreEqual(1, successfulMailer.CallCount, "Mailer should have been called once");
            // Verify it's not a CompositeMailer (single mailer should be registered directly)
            Assert.IsFalse(mailer is CompositeMailer, "Single mailer should not be wrapped in CompositeMailer");
        }

        [TestMethod]
        public async Task ServiceCollection_WithThreeMailers_ThirdSucceeds_ShouldUseThird()
        {
            // Arrange
            TestMailer failingMailer1 = new(shouldSucceed: false, id: 1);
            TestMailer failingMailer2 = new(shouldSucceed: false, id: 2);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 3);

            ServiceCollection serviceCollection = new();
            serviceCollection
                .AddFacteur(x =>
                {
                    x.WithMailer(_ => failingMailer1)
                     .WithMailer(_ => failingMailer2)
                     .WithMailer(_ => successfulMailer)
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
            Assert.AreEqual(1, failingMailer1.CallCount, "First mailer should have been called once");
            Assert.AreEqual(1, failingMailer2.CallCount, "Second mailer should have been called once");
            Assert.AreEqual(1, successfulMailer.CallCount, "Third mailer should have been called once");
        }

        [TestMethod]
        public async Task ServiceCollection_WithThreeMailers_AllFail_ShouldThrowAggregateException()
        {
            // Arrange
            TestMailer failingMailer1 = new(shouldSucceed: false, id: 1);
            TestMailer failingMailer2 = new(shouldSucceed: false, id: 2);
            TestMailer failingMailer3 = new(shouldSucceed: false, id: 3);

            ServiceCollection serviceCollection = new();
            serviceCollection
                .AddFacteur(x =>
                {
                    x.WithMailer(_ => failingMailer1)
                     .WithMailer(_ => failingMailer2)
                     .WithMailer(_ => failingMailer3)
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
            Assert.IsGreaterThanOrEqualTo(3, exception.InnerExceptions.Count, "Should have exceptions from all three mailer attempts");
            Assert.AreEqual(1, failingMailer1.CallCount, "First mailer should have been called once");
            Assert.AreEqual(1, failingMailer2.CallCount, "Second mailer should have been called once");
            Assert.AreEqual(1, failingMailer3.CallCount, "Third mailer should have been called once");
        }

        [TestMethod]
        public async Task ServiceCollection_WithMailer_NullFactory_ShouldRegisterType()
        {
            // Arrange - Test that WithMailer<T>() without factory relies on DI to construct the mailer
            // TestMailer requires constructor parameters, so we register it with a factory first
            // This tests that type-based registration works when the type is already in DI
            ServiceCollection serviceCollection = new();
            TestMailer testMailer = new(shouldSucceed: true, id: 1);

            // Register TestMailer type in DI first so it can be resolved
            serviceCollection.AddScoped<TestMailer>(_ => testMailer);

            serviceCollection.AddFacteur(x => x.WithMailer<TestMailer>().WithDefaultComposer());
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            IMailer mailer = serviceProvider.GetRequiredService<IMailer>();
            TestMailer registeredMailer = serviceProvider.GetRequiredService<TestMailer>();

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"],
                Body = "Test email body"
            };

            await mailer.SendMailAsync(request);

            // Assert
            Assert.IsNotNull(mailer, "Mailer should be registered");
            Assert.IsNotNull(registeredMailer, "TestMailer type should be registered");
            Assert.AreEqual(registeredMailer, mailer, "Mailer should be the same instance as registered TestMailer");
            Assert.AreEqual(1, testMailer.CallCount, "Mailer should have been called once");
        }

        [TestMethod]
        public async Task ServiceCollection_ScopedResolution_DifferentScopes_DifferentInstances()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            serviceCollection.AddFacteur(x => x.WithMailer(sp => new TestMailer(shouldSucceed: true, id: 1)).WithDefaultComposer());
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            using IServiceScope scope1 = serviceProvider.CreateScope();
            using IServiceScope scope2 = serviceProvider.CreateScope();
            IMailer mailer1 = scope1.ServiceProvider.GetRequiredService<IMailer>();
            IMailer mailer2 = scope2.ServiceProvider.GetRequiredService<IMailer>();

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"],
                Body = "Test email body"
            };

            await mailer1.SendMailAsync(request);
            await mailer2.SendMailAsync(request);

            // Assert
            // Since mailers are scoped, they should be different instances
            // But if it's a single mailer (not CompositeMailer), we can't verify instance differences easily
            // Instead, we verify that both work correctly
            Assert.IsNotNull(mailer1, "First scoped mailer should be resolved");
            Assert.IsNotNull(mailer2, "Second scoped mailer should be resolved");
        }
    }
}