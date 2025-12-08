using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facteur.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Retry;

namespace Facteur.Extensions.DependencyInjection.Tests
{
    [TestClass]
    public class FailoverMailerTests
    {
        [TestMethod]
        public void FailoverMailer_Constructor_NullMailers_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new FailoverMailer((IEnumerable<IMailer>)null!));
        }

        [TestMethod]
        public void FailoverMailer_Constructor_EmptyMailers_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => new FailoverMailer(Array.Empty<IMailer>()));
        }

        [TestMethod]
        public async Task FailoverMailer_SendMail_FirstMailerSucceeds_ShouldNotTryOthers()
        {
            // Arrange
            TestMailer successfulMailer = new(shouldSucceed: true, id: 1);
            TestMailer fallbackMailer = new(shouldSucceed: true, id: 2);
            FailoverMailer failoverMailer = new([successfulMailer, fallbackMailer]);

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            // Act
            await failoverMailer.SendMailAsync(request);

            // Assert
            Assert.AreEqual(1, successfulMailer.CallCount, "First mailer should have been called");
            Assert.AreEqual(0, fallbackMailer.CallCount, "Fallback mailer should not have been called");
        }

        [TestMethod]
        public async Task FailoverMailer_SendMail_FirstFailsSecondSucceeds_ShouldUseSecond()
        {
            // Arrange
            TestMailer failingMailer = new(shouldSucceed: false, id: 1);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 2);
            FailoverMailer failoverMailer = new([failingMailer, successfulMailer]);

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            // Act
            await failoverMailer.SendMailAsync(request);

            // Assert
            // Without retries, failing mailer will be called once, then we move to the next
            Assert.AreEqual(1, failingMailer.CallCount, "First mailer should have been called once");
            Assert.AreEqual(1, successfulMailer.CallCount, "Second mailer should have been called once");
        }

        [TestMethod]
        public async Task FailoverMailer_SendMail_AllFail_ShouldThrowAggregateException()
        {
            // Arrange
            TestMailer failingMailer1 = new(shouldSucceed: false, id: 1);
            TestMailer failingMailer2 = new(shouldSucceed: false, id: 2);
            FailoverMailer failoverMailer = new([failingMailer1, failingMailer2]);

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            // Act & Assert
            AggregateException exception = await Assert.ThrowsAsync<AggregateException>(() => failoverMailer.SendMailAsync(request));

            // Without retries, each mailer is tried once
            Assert.HasCount(2, exception.InnerExceptions, "Should have exactly one exception from each mailer");
            Assert.AreEqual(1, failingMailer1.CallCount, "First mailer should have been called once");
            Assert.AreEqual(1, failingMailer2.CallCount, "Second mailer should have been called once");
        }

        [TestMethod]
        public async Task FailoverMailer_SendMail_WithCompose_FirstFailsSecondSucceeds_ShouldUseSecond()
        {
            // Arrange
            TestMailer failingMailer = new(shouldSucceed: false, id: 1);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 2);
            FailoverMailer failoverMailer = new([failingMailer, successfulMailer]);

            // Act
            await failoverMailer.SendMailAsync(async composer =>
            {
                return await Task.FromResult(new EmailRequest
                {
                    Subject = "Test",
                    From = new Sender("test@example.com", "Test"),
                    To = ["recipient@example.com"]
                });
            });

            // Assert
            // Without retries, failing mailer will be called once, then we move to the next
            Assert.AreEqual(1, failingMailer.CallCount, "First mailer should have been called once");
            Assert.AreEqual(1, successfulMailer.CallCount, "Second mailer should have been called once");
        }

        [TestMethod]
        public async Task FailoverMailer_WithRetryPolicy_MailerRetriesBeforeFailing()
        {
            // Arrange
            TestMailer failingMailer = new(shouldSucceed: false, id: 1);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 2);

            // Create retry policy with 2 retries (3 total attempts)
            ResiliencePipeline retryPolicy = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .Build();

            // Create mailer entries with retry policies
            RetryableMailerEntry entry1 = new(failingMailer, async (func) => await retryPolicy.ExecuteAsync(async _ => await func()));
            RetryableMailerEntry entry2 = new(successfulMailer, async (func) => await Task.Run(func));

            FailoverMailer failoverMailer = new([entry1, entry2]);

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            // Act
            await failoverMailer.SendMailAsync(request);

            // Assert
            // With 2 retries, the failing mailer should be called 3 times (initial + 2 retries)
            Assert.AreEqual(3, failingMailer.CallCount, "First mailer should have been called 3 times (1 initial + 2 retries)");
            Assert.AreEqual(1, successfulMailer.CallCount, "Second mailer should have been called once after first mailer exhausted retries");
        }

        [TestMethod]
        public async Task FailoverMailer_WithRetryPolicy_FirstMailerSucceedsOnRetry_ShouldNotTrySecond()
        {
            // Arrange
            TestMailer retryingMailer = new(shouldSucceed: false, id: 1, succeedOnAttempt: 2); // Succeeds on 2nd attempt
            TestMailer fallbackMailer = new(shouldSucceed: true, id: 2);

            // Create retry policy with 2 retries
            ResiliencePipeline retryPolicy = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .Build();

            RetryableMailerEntry entry1 = new(retryingMailer, async (func) => await retryPolicy.ExecuteAsync(async _ => await func()));
            RetryableMailerEntry entry2 = new(fallbackMailer, async (func) => await Task.Run(func));

            FailoverMailer failoverMailer = new([entry1, entry2]);

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            // Act
            await failoverMailer.SendMailAsync(request);

            // Assert
            // Should succeed on 2nd attempt, so 2 calls total (1 initial + 1 retry)
            Assert.AreEqual(2, retryingMailer.CallCount, "First mailer should have been called twice (succeeds on retry)");
            Assert.AreEqual(0, fallbackMailer.CallCount, "Fallback mailer should not have been called");
        }

        [TestMethod]
        public async Task FailoverMailer_WithRetryPolicy_AllMailersFail_ShouldThrowAggregateException()
        {
            // Arrange
            TestMailer failingMailer1 = new(shouldSucceed: false, id: 1);
            TestMailer failingMailer2 = new(shouldSucceed: false, id: 2);

            // Create retry policy with 1 retry (2 total attempts)
            ResiliencePipeline retryPolicy = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 1,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .Build();

            RetryableMailerEntry entry1 = new(failingMailer1, async (func) => await retryPolicy.ExecuteAsync(async _ => await func()));
            RetryableMailerEntry entry2 = new(failingMailer2, async (func) => await retryPolicy.ExecuteAsync(async _ => await func()));

            FailoverMailer failoverMailer = new([entry1, entry2]);

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            // Act & Assert
            AggregateException exception = await Assert.ThrowsAsync<AggregateException>(() => failoverMailer.SendMailAsync(request));

            // Each mailer should have been called twice (1 initial + 1 retry)
            Assert.HasCount(2, exception.InnerExceptions, "Should have exceptions from both mailers");
            Assert.AreEqual(2, failingMailer1.CallCount, "First mailer should have been called twice");
            Assert.AreEqual(2, failingMailer2.CallCount, "Second mailer should have been called twice");
        }

        [TestMethod]
        public async Task FailoverMailer_WithMailersAPI_RetryPolicyIntegration_ShouldRetryBeforeFailover()
        {
            // Arrange
            ServiceCollection services = new();
            TestMailer failingMailer = new(shouldSucceed: false, id: 1);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 2);

            FacteurBuilder builder = new(services);
            builder.WithMailers(config =>
            {
                config.WithMailer(_ => failingMailer)
                    .WithRetryPolicy(policy =>
                    {
                        policy.AddRetry(new RetryStrategyOptions
                        {
                            MaxRetryAttempts = 2,
                            ShouldHandle = new PredicateBuilder().Handle<Exception>()
                        });
                    });

                config.WithMailer(_ => successfulMailer)
                    .WithRetryPolicy(policy =>
                    {
                        policy.AddRetry(new RetryStrategyOptions
                        {
                            MaxRetryAttempts = 1,
                            ShouldHandle = new PredicateBuilder().Handle<Exception>()
                        });
                    });
            });

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMailer mailer = serviceProvider.GetRequiredService<IMailer>();

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            // Act
            await mailer.SendMailAsync(request);

            // Assert
            // First mailer should retry 2 times (3 total calls) before failing and moving to second mailer
            Assert.AreEqual(3, failingMailer.CallCount, "First mailer should have been called 3 times (1 initial + 2 retries)");
            Assert.AreEqual(1, successfulMailer.CallCount, "Second mailer should have been called once after first mailer exhausted retries");
        }

        [TestMethod]
        public async Task FailoverMailer_MixedRetryPolicies_SomeWithRetrySomeWithout_ShouldWork()
        {
            // Arrange
            TestMailer mailerWithoutRetry = new(shouldSucceed: false, id: 1);
            TestMailer mailerWithRetry = new(shouldSucceed: false, id: 2);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 3);

            // Create retry policy with 1 retry (2 total attempts)
            ResiliencePipeline retryPolicy = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 1,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .Build();

            // Mix entries: one without retry, one with retry, one without retry
            RetryableMailerEntry entry1 = new(mailerWithoutRetry, async (func) => await func()); // No retry
            RetryableMailerEntry entry2 = new(mailerWithRetry, async (func) => await retryPolicy.ExecuteAsync(async _ => await func())); // With retry
            RetryableMailerEntry entry3 = new(successfulMailer, async (func) => await func()); // No retry

            FailoverMailer failoverMailer = new([entry1, entry2, entry3]);

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            // Act
            await failoverMailer.SendMailAsync(request);

            // Assert

            // First mailer: no retry, called once
            Assert.AreEqual(1, mailerWithoutRetry.CallCount, "Mailer without retry should be called once");

            // Second mailer: with retry (1 retry = 2 total attempts)
            Assert.AreEqual(2, mailerWithRetry.CallCount, "Mailer with retry should be called twice (1 initial + 1 retry)");

            // Third mailer: succeeds, called once
            Assert.AreEqual(1, successfulMailer.CallCount, "Successful mailer should be called once");
        }
    }
}