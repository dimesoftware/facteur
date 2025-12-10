using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Facteur.Extensions.DependencyInjection;
using Facteur.TemplateProviders.IO;
using Facteur.Tests;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace Facteur.Extensions.DependencyInjection.Resiliency.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ServiceCollectionTests
    {
        [TestMethod]
        public async Task CompositeMailer_WithMailersAPI_RetryPolicyIntegration_ShouldRetryBeforeFailover()
        {
            // Arrange
            ServiceCollection services = new();
            TestMailer failingMailer = new(shouldSucceed: false, id: 1);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 2);

            FacteurBuilder builder = new(services);
            builder.WithMailer(
                    _ => failingMailer,
                    policy => policy.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 2,
                        Delay = TimeSpan.Zero,
                        ShouldHandle = new PredicateBuilder().Handle<Exception>()
                    }))
                .WithMailer(
                    _ => successfulMailer,
                    policy => policy.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 1,
                        Delay = TimeSpan.Zero,
                        ShouldHandle = new PredicateBuilder().Handle<Exception>()
                    })
                );

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
        public async Task CompositeMailer_WithMailersAPI_WithInlineRetryPolicy_ShouldWork()
        {
            // Arrange
            ServiceCollection services = new();
            TestMailer failingMailer = new(shouldSucceed: false, id: 1);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 2);

            FacteurBuilder builder = new(services);
            builder.WithMailer(
                    _ => failingMailer,
                    policy =>
                    {
                        policy.AddRetry(new RetryStrategyOptions
                        {
                            MaxRetryAttempts = 2,
                            ShouldHandle = new PredicateBuilder().Handle<Exception>()
                        });
                    })
                .WithMailer(
                    _ => successfulMailer,
                    policy =>
                    {
                        policy.AddRetry(new RetryStrategyOptions
                        {
                            MaxRetryAttempts = 1,
                            ShouldHandle = new PredicateBuilder().Handle<Exception>()
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
        public async Task CompositeMailer_MixedRetryPolicies_SomeWithRetrySomeWithout_ShouldWork()
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
                    Delay = TimeSpan.Zero,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .Build();

            // Mix entries: one without retry, one with retry, one without retry
            MailerEntry entry1 = new(mailerWithoutRetry, async (func) => await func()); // No retry
            MailerEntry entry2 = new(mailerWithRetry, async (func) => await retryPolicy.ExecuteAsync(async _ => await func())); // With retry
            MailerEntry entry3 = new(successfulMailer, async (func) => await func()); // No retry

            CompositeMailer compositeMailer = new([entry1, entry2, entry3]);

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            // Act
            await compositeMailer.SendMailAsync(request);

            // Assert

            // First mailer: no retry, called once
            Assert.AreEqual(1, mailerWithoutRetry.CallCount, "Mailer without retry should be called once");

            // Second mailer: with retry (1 retry = 2 total attempts)
            Assert.AreEqual(2, mailerWithRetry.CallCount, "Mailer with retry should be called twice (1 initial + 1 retry)");

            // Third mailer: succeeds, called once
            Assert.AreEqual(1, successfulMailer.CallCount, "Successful mailer should be called once");
        }

        [TestMethod]
        public async Task CompositeMailer_WithMailersAPI_SingleMailerWithoutPolicy_ShouldWork()
        {
            // Arrange
            ServiceCollection services = new();
            TestMailer successfulMailer = new(shouldSucceed: true, id: 1);

            FacteurBuilder builder = new(services);
            builder.WithMailer(_ => successfulMailer, configurePolicy: null);

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
            Assert.AreEqual(1, successfulMailer.CallCount, "Mailer should have been called once");
            // Without policy, single mailer should be registered directly (not wrapped)
            Assert.IsFalse(mailer is CompositeMailer, "Single mailer without policy should not be wrapped in CompositeMailer");
        }

        [TestMethod]
        public async Task CompositeMailer_WithMailersAPI_SingleMailerWithPolicy_ShouldWrapInComposite()
        {
            // Arrange
            ServiceCollection services = new();
            TestMailer successfulMailer = new(shouldSucceed: true, id: 1);

            FacteurBuilder builder = new(services);
            builder.WithMailer(
                _ => successfulMailer,
                policy => policy.AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 1,
                    Delay = TimeSpan.Zero,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                }));

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
            Assert.AreEqual(1, successfulMailer.CallCount, "Mailer should have been called once");
            // With policy, single mailer should be wrapped in CompositeMailer
            Assert.IsTrue(mailer is CompositeMailer, "Single mailer with policy should be wrapped in CompositeMailer");
        }

        [TestMethod]
        public async Task ServiceCollection_WithMailer_NullFactory_TypeBasedRegistration_ShouldWork()
        {
            // Arrange
            ServiceCollection services = new();
            TestMailer testMailer = new(shouldSucceed: true, id: 1);
            services.AddScoped<TestMailer>(_ => testMailer);

            FacteurBuilder builder = new(services);
            builder.WithMailer<TestMailer>(factory: null, configurePolicy: null);

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
            Assert.AreEqual(1, testMailer.CallCount, "Mailer should have been called once");
        }

        [TestMethod]
        public async Task CompositeMailer_WithMailersAPI_MultipleMailers_SomeWithNullPolicy_ShouldWork()
        {
            // Arrange
            ServiceCollection services = new();
            TestMailer mailerWithoutPolicy = new(shouldSucceed: false, id: 1);
            TestMailer mailerWithPolicy = new(shouldSucceed: false, id: 2);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 3);

            FacteurBuilder builder = new(services);
            builder.WithMailer(_ => mailerWithoutPolicy, configurePolicy: null) // No policy
                .WithMailer(
                    _ => mailerWithPolicy,
                    policy => policy.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 1,
                        Delay = TimeSpan.Zero,
                        ShouldHandle = new PredicateBuilder().Handle<Exception>()
                    }))
                .WithMailer(_ => successfulMailer, configurePolicy: null); // No policy

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
            // First mailer: no policy, called once
            Assert.AreEqual(1, mailerWithoutPolicy.CallCount, "Mailer without policy should be called once");

            // Second mailer: with policy (1 retry = 2 total attempts)
            Assert.AreEqual(2, mailerWithPolicy.CallCount, "Mailer with policy should be called twice (1 initial + 1 retry)");

            // Third mailer: succeeds, called once
            Assert.AreEqual(1, successfulMailer.CallCount, "Successful mailer should be called once");
        }

        [TestMethod]
        public async Task CompositeMailer_WithMailersAPI_MultipleMailers_FirstSucceedsOnRetry_ShouldNotTryOthers()
        {
            // Arrange
            ServiceCollection services = new();
            TestMailer retryingMailer = new(shouldSucceed: false, id: 1, succeedOnAttempt: 2); // Succeeds on 2nd attempt
            TestMailer fallbackMailer = new(shouldSucceed: true, id: 2);

            FacteurBuilder builder = new(services);
            builder.WithMailer(
                    _ => retryingMailer,
                    policy => policy.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 2,
                        Delay = TimeSpan.Zero,
                        ShouldHandle = new PredicateBuilder().Handle<Exception>()
                    }))
                .WithMailer(_ => fallbackMailer, configurePolicy: null);

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
            // Should succeed on 2nd attempt, so 2 calls total (1 initial + 1 retry)
            Assert.AreEqual(2, retryingMailer.CallCount, "First mailer should have been called twice (succeeds on retry)");
            Assert.AreEqual(0, fallbackMailer.CallCount, "Fallback mailer should not have been called");
        }

        [TestMethod]
        public async Task CompositeMailer_WithMailersAPI_ScopedResolution_DifferentScopes_ShouldWork()
        {
            // Arrange
            ServiceCollection services = new();
            TestMailer successfulMailer = new(shouldSucceed: true, id: 1);

            FacteurBuilder builder = new(services);
            builder.WithMailer(_ => new TestMailer(shouldSucceed: true, id: 1), configurePolicy: null);

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act
            using IServiceScope scope1 = serviceProvider.CreateScope();
            using IServiceScope scope2 = serviceProvider.CreateScope();
            IMailer mailer1 = scope1.ServiceProvider.GetRequiredService<IMailer>();
            IMailer mailer2 = scope2.ServiceProvider.GetRequiredService<IMailer>();

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            await mailer1.SendMailAsync(request);
            await mailer2.SendMailAsync(request);

            // Assert
            Assert.IsNotNull(mailer1, "First scoped mailer should be resolved");
            Assert.IsNotNull(mailer2, "Second scoped mailer should be resolved");
        }

        [TestMethod]
        public async Task CompositeMailer_WithMailersAPI_MultipleBuilderInstances_ShouldNotInterfere()
        {
            // Arrange
            ServiceCollection services1 = new();
            ServiceCollection services2 = new();
            TestMailer mailer1 = new(shouldSucceed: true, id: 1);
            TestMailer mailer2 = new(shouldSucceed: true, id: 2);

            FacteurBuilder builder1 = new(services1);
            FacteurBuilder builder2 = new(services2);

            builder1.WithMailer(_ => mailer1, configurePolicy: null);
            builder2.WithMailer(_ => mailer2, configurePolicy: null);

            ServiceProvider serviceProvider1 = services1.BuildServiceProvider();
            ServiceProvider serviceProvider2 = services2.BuildServiceProvider();

            EmailRequest request = new()
            {
                Subject = "Test",
                From = new Sender("test@example.com", "Test"),
                To = ["recipient@example.com"]
            };

            // Act
            IMailer resolvedMailer1 = serviceProvider1.GetRequiredService<IMailer>();
            IMailer resolvedMailer2 = serviceProvider2.GetRequiredService<IMailer>();

            await resolvedMailer1.SendMailAsync(request);
            await resolvedMailer2.SendMailAsync(request);

            // Assert
            Assert.AreEqual(1, mailer1.CallCount, "First builder's mailer should have been called once");
            Assert.AreEqual(1, mailer2.CallCount, "Second builder's mailer should have been called once");
        }

        [TestMethod]
        public async Task CompositeMailer_WithMailersAPI_ComposeEmail_WithRetryPolicy_ShouldWork()
        {
            // Arrange
            ServiceCollection services = new();
            TestMailer failingMailer = new(shouldSucceed: false, id: 1);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 2);

            FacteurBuilder builder = new(services);
            builder.WithMailer(
                    _ => failingMailer,
                    policy => policy.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 1,
                        Delay = TimeSpan.Zero,
                        ShouldHandle = new PredicateBuilder().Handle<Exception>()
                    }))
                .WithMailer(_ => successfulMailer, configurePolicy: null);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMailer mailer = serviceProvider.GetRequiredService<IMailer>();

            // Act
            await mailer.SendMailAsync(async composer =>
            {
                return await Task.FromResult(new EmailRequest
                {
                    Subject = "Test",
                    From = new Sender("test@example.com", "Test"),
                    To = ["recipient@example.com"]
                });
            });

            // Assert
            // First mailer: with retry (1 retry = 2 total attempts)
            Assert.AreEqual(2, failingMailer.CallCount, "Failing mailer with policy should be called twice");
            Assert.AreEqual(1, successfulMailer.CallCount, "Successful mailer should be called once");
        }

        [TestMethod]
        public async Task ServiceCollection_WithMailers_RetryPolicy_AllFail_ShouldThrowAggregateException()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            TestMailer failingMailer1 = new(shouldSucceed: false, id: 1);
            TestMailer failingMailer2 = new(shouldSucceed: false, id: 2);

            serviceCollection.AddFacteur(x => x
                .WithMailer(
                    _ => failingMailer1,
                    policy => policy.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 1,
                        Delay = TimeSpan.Zero,
                        ShouldHandle = new PredicateBuilder().Handle<Exception>()
                    }))
                .WithMailer(
                    _ => failingMailer2,
                    policy => policy.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 1,
                        Delay = TimeSpan.Zero,
                        ShouldHandle = new PredicateBuilder().Handle<Exception>()
                    }))
                .WithCompiler<ScribanCompiler>()
                .WithTemplateProvider(_ => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
                .WithResolver<ViewModelTemplateResolver>()
                .WithDefaultComposer()
            );

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
            // Each mailer should have been called twice (1 initial + 1 retry)
            Assert.AreEqual(2, failingMailer1.CallCount, "First mailer should have been called twice");
            Assert.AreEqual(2, failingMailer2.CallCount, "Second mailer should have been called twice");
        }

        [TestMethod]
        public async Task ServiceCollection_WithMailers_RetryPolicy_ComposeEmail_ShouldWork()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            TestMailer failingMailer = new(shouldSucceed: false, id: 1);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 2);

            serviceCollection
                .AddFacteur(x =>
                {
                    x.WithMailer(
                            _ => failingMailer,
                            policy => policy.AddRetry(new RetryStrategyOptions
                            {
                                MaxRetryAttempts = 1,
                                ShouldHandle = new PredicateBuilder().Handle<Exception>()
                            }))
                        .WithMailer(_ => successfulMailer, configurePolicy: null)
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
            Assert.AreEqual(2, failingMailer.CallCount, "Failing mailer with policy should be called twice (1 initial + 1 retry)");
            Assert.AreEqual(1, successfulMailer.CallCount, "Successful mailer should be called once");
        }

        [TestMethod]
        public async Task ServiceCollection_WithSingleMailer_WithRetryPolicy_ShouldWork()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            TestMailer successfulMailer = new(shouldSucceed: true, id: 1);

            serviceCollection
                .AddFacteur(x =>
                {
                    x.WithMailer(
                            _ => successfulMailer,
                            policy => policy.AddRetry(new RetryStrategyOptions
                            {
                                MaxRetryAttempts = 1,
                                ShouldHandle = new PredicateBuilder().Handle<Exception>()
                            }))
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
            // With policy, single mailer should be wrapped in CompositeMailer
            Assert.IsTrue(mailer is CompositeMailer, "Single mailer with policy should be wrapped in CompositeMailer");
        }

        [TestMethod]
        public async Task ServiceCollection_WithThreeMailers_RetryPolicy_ThirdSucceeds_ShouldUseThird()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            TestMailer failingMailer1 = new(shouldSucceed: false, id: 1);
            TestMailer failingMailer2 = new(shouldSucceed: false, id: 2);
            TestMailer successfulMailer = new(shouldSucceed: true, id: 3);

            serviceCollection
                .AddFacteur(x =>
                {
                    x.WithMailer(
                            _ => failingMailer1,
                            policy => policy.AddRetry(new RetryStrategyOptions
                            {
                                MaxRetryAttempts = 1,
                                ShouldHandle = new PredicateBuilder().Handle<Exception>()
                            }))
                        .WithMailer(
                            _ => failingMailer2,
                            policy => policy.AddRetry(new RetryStrategyOptions
                            {
                                MaxRetryAttempts = 1,
                                ShouldHandle = new PredicateBuilder().Handle<Exception>()
                            }))
                        .WithMailer(_ => successfulMailer, configurePolicy: null)
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
            Assert.AreEqual(2, failingMailer1.CallCount, "First mailer should have been called twice (1 initial + 1 retry)");
            Assert.AreEqual(2, failingMailer2.CallCount, "Second mailer should have been called twice (1 initial + 1 retry)");
            Assert.AreEqual(1, successfulMailer.CallCount, "Third mailer should have been called once");
        }

        [TestMethod]
        public async Task ServiceCollection_WithMailer_TypeBasedRegistration_ReliesOnDI_ShouldWork()
        {
            // Arrange - Test that WithMailer<T>() without factory relies on DI to construct the mailer
            // This mailer has a parameterless constructor so DI can construct it automatically
            ServiceCollection services = new();

            FacteurBuilder builder = new(services);
            // Call WithMailer without factory - it should use DI to resolve SimpleTestMailer
            builder.WithMailer<SimpleTestMailer>(factory: null, configurePolicy: null);

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
            // Verify that SimpleTestMailer was constructed by DI (not manually created)
            SimpleTestMailer resolvedMailer = serviceProvider.GetRequiredService<SimpleTestMailer>();
            Assert.IsNotNull(resolvedMailer, "SimpleTestMailer should be resolved from DI");
            Assert.AreEqual(mailer, resolvedMailer, "IMailer should be the same instance as SimpleTestMailer");
            Assert.AreEqual(1, resolvedMailer.CallCount, "Mailer should have been called once");
        }

        [TestMethod]
        public async Task ServiceCollection_WithMailer_TypeBasedRegistration_WithRetryPolicy_ReliesOnDI_ShouldWork()
        {
            // Arrange - Test type-based registration with retry policy
            ServiceCollection services = new();

            FacteurBuilder builder = new(services);
            builder.WithMailer<SimpleTestMailer>(
                factory: null,
                policy => policy.AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 1,
                    Delay = TimeSpan.Zero,
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                }));

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
            SimpleTestMailer resolvedMailer = serviceProvider.GetRequiredService<SimpleTestMailer>();
            Assert.IsNotNull(resolvedMailer, "SimpleTestMailer should be resolved from DI");
            Assert.AreEqual(1, resolvedMailer.CallCount, "Mailer should have been called once");
            // With policy, single mailer should be wrapped in CompositeMailer
            Assert.IsTrue(mailer is CompositeMailer, "Single mailer with policy should be wrapped in CompositeMailer");
        }
    }
}