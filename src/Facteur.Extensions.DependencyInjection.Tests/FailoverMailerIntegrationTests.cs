using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.Smtp;
using Facteur.TemplateProviders.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Retry;

namespace Facteur.Extensions.DependencyInjection.Tests
{
    /// <summary>
    /// Integration tests that use real mailer implementations within an IHost application to verify failover behavior.
    /// These tests require environment variables to be set:
    /// - TEST_SMTP_EMAIL: SMTP username
    /// - TEST_SMTP_PASSWORD: SMTP password
    /// - TEST_SMTP_HOST (optional): SMTP host (defaults to sandbox.smtp.mailtrap.io)
    /// - TEST_SMTP_PORT (optional): SMTP port (defaults to 2525)
    /// 
    /// Note: These tests are marked with [Ignore] by default to prevent accidental execution
    /// in CI/CD pipelines. Uncomment [Ignore] or run manually with appropriate test credentials.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FailoverMailerIntegrationTests
    {
        [TestMethod]
        [Ignore("Integration test - requires SMTP credentials. Set environment variables and remove [Ignore] to run.")]
        public async Task FailoverMailer_WithHost_FirstFailsSecondSucceeds_ShouldUseSecond()
        {
            // Arrange
            string testEmail = Environment.GetEnvironmentVariable("TEST_SMTP_EMAIL");
            string testPw = Environment.GetEnvironmentVariable("TEST_SMTP_PASSWORD");
            string smtpHost = Environment.GetEnvironmentVariable("TEST_SMTP_HOST") ?? "sandbox.smtp.mailtrap.io";
            string smtpPort = Environment.GetEnvironmentVariable("TEST_SMTP_PORT") ?? "2525";

            if (string.IsNullOrEmpty(testEmail) || string.IsNullOrEmpty(testPw))
            {
                Assert.Inconclusive("TEST_SMTP_EMAIL and TEST_SMTP_PASSWORD environment variables must be set.");
                return;
            }

            // First mailer with invalid credentials (will fail)
            SmtpCredentials invalidCredentials = new("invalid-host.example.com", "2525", "false", "true", "invalid", "invalid");
            
            // Second mailer with valid credentials (will succeed)
            SmtpCredentials validCredentials = new(smtpHost, smtpPort, "false", "true", testEmail, testPw);

            // Build an actual IHost application
            IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddFacteur(x =>
                        {
                            x.WithMailers(config =>
                            {
                                // First mailer: Invalid credentials, no retry - should fail immediately
                                config.WithMailer(sp => new SmtpMailer(invalidCredentials, sp.GetService<IEmailComposer>()))
                                    .WithoutRetryPolicy();

                                // Second mailer: Valid credentials, with retry policy
                                config.WithMailer(sp => new SmtpMailer(validCredentials, sp.GetService<IEmailComposer>()))
                                    .WithRetryPolicy(policy =>
                                    {
                                        policy.AddRetry(new RetryStrategyOptions
                                        {
                                            MaxRetryAttempts = 2,
                                            ShouldHandle = new PredicateBuilder().Handle<Exception>()
                                        });
                                    });
                            })
                            .WithCompiler<ScribanCompiler>()
                            .WithTemplateProvider(_ => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                            .WithResolver<ViewModelTemplateResolver>()
                            .WithDefaultComposer();
                        });
                })
                .Build();

            // Act
            using (host)
            {
                IMailer mailer = host.Services.GetRequiredService<IMailer>();

                EmailRequest request = new()
                {
                    Subject = "Integration Test - Failover with IHost",
                    From = new Sender("test@example.com", "Test Sender"),
                    To = ["test@example.com"],
                    Body = "This is a test email to verify failover behavior through IHost."
                };

                await mailer.SendMailAsync(request);
            }

            // Assert
            // If we reach here, the second mailer succeeded after the first failed
            Assert.IsTrue(true, "Email should have been sent via the second (valid) mailer");
        }

        [TestMethod]
        [Ignore("Integration test - requires SMTP credentials. Set environment variables and remove [Ignore] to run.")]
        public async Task FailoverMailer_WithHost_MixedRetryPolicies_ShouldWork()
        {
            // Arrange
            string testEmail = Environment.GetEnvironmentVariable("TEST_SMTP_EMAIL");
            string testPw = Environment.GetEnvironmentVariable("TEST_SMTP_PASSWORD");
            string smtpHost = Environment.GetEnvironmentVariable("TEST_SMTP_HOST") ?? "sandbox.smtp.mailtrap.io";
            string smtpPort = Environment.GetEnvironmentVariable("TEST_SMTP_PORT") ?? "2525";

            if (string.IsNullOrEmpty(testEmail) || string.IsNullOrEmpty(testPw))
            {
                Assert.Inconclusive("TEST_SMTP_EMAIL and TEST_SMTP_PASSWORD environment variables must be set.");
                return;
            }

            // First mailer: Invalid credentials, no retry
            SmtpCredentials invalidCredentials = new("invalid-host.example.com", "2525", "false", "true", "invalid", "invalid");
            
            // Second mailer: Invalid credentials, with retry (will still fail but will retry)
            SmtpCredentials invalidCredentials2 = new("another-invalid-host.example.com", "2525", "false", "true", "invalid", "invalid");
            
            // Third mailer: Valid credentials, no retry needed since it will succeed
            SmtpCredentials validCredentials = new(smtpHost, smtpPort, "false", "true", testEmail, testPw);

            // Build an actual IHost application
            IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddFacteur(x =>
                        {
                            x.WithMailers(config =>
                            {
                                // First mailer: No retry
                                config.WithMailer(sp => new SmtpMailer(invalidCredentials, sp.GetService<IEmailComposer>()))
                                    .WithoutRetryPolicy();

                                // Second mailer: With retry (will retry but still fail)
                                config.WithMailer(sp => new SmtpMailer(invalidCredentials2, sp.GetService<IEmailComposer>()))
                                    .WithRetryPolicy(policy =>
                                    {
                                        policy.AddRetry(new RetryStrategyOptions
                                        {
                                            MaxRetryAttempts = 1,
                                            ShouldHandle = new PredicateBuilder().Handle<Exception>()
                                        });
                                    });

                                // Third mailer: Valid, no retry needed
                                config.WithMailer(sp => new SmtpMailer(validCredentials, sp.GetService<IEmailComposer>()))
                                    .WithoutRetryPolicy();
                            })
                            .WithCompiler<ScribanCompiler>()
                            .WithTemplateProvider(_ => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                            .WithResolver<ViewModelTemplateResolver>()
                            .WithDefaultComposer();
                        });
                })
                .Build();

            // Act
            using (host)
            {
                IMailer mailer = host.Services.GetRequiredService<IMailer>();

                EmailRequest request = new()
                {
                    Subject = "Integration Test - Mixed Retry Policies with IHost",
                    From = new Sender("test@example.com", "Test Sender"),
                    To = ["test@example.com"],
                    Body = "This is a test email to verify mixed retry policy behavior through IHost."
                };

                await mailer.SendMailAsync(request);
            }

            // Assert
            // If we reach here, the third mailer succeeded after the first two failed
            Assert.IsTrue(true, "Email should have been sent via the third (valid) mailer after first two failed");
        }

        [TestMethod]
        [Ignore("Integration test - requires SMTP credentials. Set environment variables and remove [Ignore] to run.")]
        public async Task FailoverMailer_WithHost_AllFail_ShouldThrowAggregateException()
        {
            // Arrange
            // All mailers with invalid credentials
            SmtpCredentials invalidCredentials1 = new("invalid-host-1.example.com", "2525", "false", "true", "invalid", "invalid");
            SmtpCredentials invalidCredentials2 = new("invalid-host-2.example.com", "2525", "false", "true", "invalid", "invalid");

            // Build an actual IHost application
            IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddFacteur(x =>
                        {
                            x.WithMailers(config =>
                            {
                                config.WithMailer(sp => new SmtpMailer(invalidCredentials1, sp.GetService<IEmailComposer>()))
                                    .WithRetryPolicy(policy =>
                                    {
                                        policy.AddRetry(new RetryStrategyOptions
                                        {
                                            MaxRetryAttempts = 1,
                                            ShouldHandle = new PredicateBuilder().Handle<Exception>()
                                        });
                                    });

                                config.WithMailer(sp => new SmtpMailer(invalidCredentials2, sp.GetService<IEmailComposer>()))
                                    .WithRetryPolicy(policy =>
                                    {
                                        policy.AddRetry(new RetryStrategyOptions
                                        {
                                            MaxRetryAttempts = 1,
                                            ShouldHandle = new PredicateBuilder().Handle<Exception>()
                                        });
                                    });
                            })
                            .WithCompiler<ScribanCompiler>()
                            .WithTemplateProvider(_ => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                            .WithResolver<ViewModelTemplateResolver>()
                            .WithDefaultComposer();
                        });
                })
                .Build();

            // Act & Assert
            using (host)
            {
                IMailer mailer = host.Services.GetRequiredService<IMailer>();

                EmailRequest request = new()
                {
                    Subject = "Integration Test - All Fail with IHost",
                    From = new Sender("test@example.com", "Test Sender"),
                    To = ["test@example.com"],
                    Body = "This test should fail."
                };

                AggregateException exception = await Assert.ThrowsAsync<AggregateException>(
                    () => mailer.SendMailAsync(request));

                Assert.IsNotNull(exception, "Should throw AggregateException when all mailers fail");
                Assert.IsTrue(exception.InnerExceptions.Count >= 2, 
                    "Should have exceptions from all mailer attempts");
            }
        }
    }
}
