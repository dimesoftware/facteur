![](https://raw.githubusercontent.com/dimesoftware/facteur/master/assets/facteur.svg?raw=true =250)

# Facteur

Facteur (French for mailman) is a library for sending emails in .NET. Its modular approach allows you to assemble a mail system rather than having to use a take-it-or-leave it service.

Check out the **[📚 docs »](https://dimesoftware.github.io/facteur/)** for more info.

## About the project

The entire premise of this project is to provide a flexible and modular mailing and templating kit. Applications should not be bound by one specific mailing service; like when you get blacklisted by a mailing service or when the performance is unacceptable, you should be able to swap providers without having to modify a single line of code. 

This is why we created Facteur. The desire to create a flexible and vendor-independent framework is clearly reflected in the library's architecture.

There are a few moving parts:

- Composers
- Compilers
- Resolvers
- Template providers
- Endpoints

**Composers** enable you to create an email request, which contains the email variables like subject, body and the email addresses to send the mail to.

**Compilers** are a part of the email composition in that it allows to fetch a template and populate the email body with data from a custom view model. 

The templates can be stored anywhere. By default they are stored in the folder where the application is hosted but it can also be retrieved from an Azure blob, FTP drive, etc. Using **template providers** and **resolvers**, you can write your own logic to fetch the right template for the job.

Lastly and obviously, there are the various mail services, also known as **endpoints** in Facteur. emails can be sent with good old SMTP, Microsoft Graph API, Plunk, Resend, SendGrid, etc.

## Installation

Use the package manager NuGet to install the base library of Facteur:

`dotnet add package Facteur`

Next it is up to you to decide which *endpoint* you want to use:

| Service             | Command                               |
| ------------------- | ------------------------------------- |
| Microsoft Graph API | `dotnet add package Facteur.MsGraph`  |
| Plunk               | `dotnet add package Facteur.Plunk`    |
| Resend              | `dotnet add package Facteur.Resend`   |
| SMTP                | `dotnet add package Facteur.Smtp`     |
| SendGrid            | `dotnet add package Facteur.SendGrid` |

Next, you should decide which *compiler* to use to generate the body of your email. The following packages are available:

| Resolvers   | Command                                        |
| ----------- | ---------------------------------------------- |

| Scriban     | `dotnet add package Facteur.Compilers.Scriban` |

You also have a choice in the template providers. Templates can be stored on a regular file drive but it might as well be stored on a blob on Azure.

| Providers | Command                                           |
| --------- | ------------------------------------------------- |
| IO        | `dotnet add package Facteur.TemplateProviders.IO` |

The resolvers are the glue between the storage of templates and the runtime. Resolvers enable you to map templates to models.

| Resolvers | Command                                          |
| --------- | ------------------------------------------------ |
| View      | `dotnet add package Facteur.Resolvers.ViewModel` |

Finally, there are some ancillary packages:

| Purpose      | Command                                                              |
| ------------ | -------------------------------------------------------------------- |
| .NET Core DI | `dotnet add package Facteur.Extensions.DependencyInjection`          |
| Failover     | `dotnet add package Facteur.Extensions.DependencyInjection.Resiliency` |

## Usage

The power of this project is to create a dynamic mail body as you can populate any template with any type of data. This is when the compilers, providers and resolvers come in. They can be produced using the implementation of `IEmailCompiler`, which orchestrates the process of retrieving and populating the template. It is ultimately up to the instance of the `IMailer` to actually send the email.

``` csharp
public async Task SendConfirmationMail(string customerMail, string customerName)
{
  EmailComposer composer = new(
    new ScribanCompiler(),
    new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
    new ViewModelTemplateResolver());

  EmailRequest request = await composer      
      .Subject("Hello world")
      .From("info@facteur.com")
      .To("guy.gadbois@facteur.com")
      .Cc("jacques.clouseau@facteur.com")
      .Bcc("charles.dreyfus@facteur.com")
      .BuildAsync(new TestMailModel { Email = customerMail, Name = customerMail });

  SmtpCredentials credentials = new("smtp.gmail.com", "587", "false", "true", "myuser@gmail.com", "mypassword");
  IMailer mailer = new SmtpMailer(credentials);
  await mailer.SendMailAsync(request);
}
```

If you use DI, you can just use `IMailer` and use the overload that exposes the composer:

``` csharp
public async Task SendConfirmationMail(string customerMail, string customerName)
{
  await mailer.SendMailAsync(x =>  x      
      .Subject("Hello world")
      .From("info@facteur.com")
      .To("guy.gadbois@facteur.com")
      .Cc("jacques.clouseau@facteur.com")
      .Bcc("charles.dreyfus@facteur.com")
      .BuildAsync(new TestMailModel { Email = customerMail, Name = customerMail }));
}
```

This particular example uses scriban templates that are stored inside the application's directory. Inside the HTML template, you will find scriban syntax:

```html
<p>Hi {{name}},</p>
```

This text template is resolved using the model that is passed to the `EmailRequest` instance, which in this sample is of the `TestMailModel` type:

```csharp
public class TestMailModel
{
  public string Name { get; set; }
  public string Email { get; set; }
}
```

The resolver is responsible for locating the right file name. In this example, the `ViewModelTemplateResolver` is used. This class essentially strips the 'MailModel' or 'ViewModel' of the name of the mail request's model. After that, the provider (`AppDirectoryTemplateProvider`) will make the system to look for file in the application's `Templates` directory with the .sbnhtml file and with the name 'Test' (from Test~~MailModel~~).

The `IEmailComposer` brings everything together and generates a populated mail body. Then it's up to the `ÌMailer` to merely send the mail.

With .NET's dependency injection, hooking up the mailer can be done by adding a few lines to the Startup class:

```csharp
serviceCollection.AddFacteur(x =>
{
    x.WithMailer(y => new SmtpMailer(credentials, y.GetService<IEmailComposer>()))
    .WithCompiler<ScribanCompiler>()
    .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
    .WithResolver<ViewModelTemplateResolver>()
    .WithDefaultComposer();
});
```

### Resilient mailers with Polly

This package extends `FacteurBuilder` with an overload of `WithMailer` that supports Polly retry policies. This allows you to configure multiple mailers with individual retry policies for advanced failover and resilience scenarios.

When multiple mailers are added using the Resiliency package's `WithMailer` overload, they are automatically wrapped in a `CompositeMailer` that will try each mailer in sequence with their configured retry policies.

#### Basic usage with retry policies

```csharp
using Facteur.Extensions.DependencyInjection.Resiliency;
using Polly;
using Polly.Retry;

serviceCollection.AddFacteur(x =>
{
    // Primary mailer with retry policy (2 retries = 3 total attempts)
    x.WithMailer(
        sp => new SendGridMailer(sendGridCredentials),
        policy => policy.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            ShouldHandle = new PredicateBuilder().Handle<Exception>()
        }))
    // Fallback mailer (tried once if primary fails)
    .WithMailer(sp => new SmtpMailer(smtpCredentials))
    .WithCompiler<ScribanCompiler>()
    .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
    .WithResolver<ViewModelTemplateResolver>()
    .WithDefaultComposer();
});
```

In this example:
- The SendGrid mailer will be tried first with up to 2 retries (3 total attempts)
- If all SendGrid attempts fail, the SMTP mailer will be tried once
- If all mailers fail, an `AggregateException` containing all exceptions will be thrown

#### Multiple mailers with different policies

```csharp
serviceCollection.AddFacteur(x =>
{
    x.WithMailer(
        sp => new SendGridMailer(sendGridCredentials),
        policy => policy.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            ShouldHandle = new PredicateBuilder().Handle<Exception>()
        }))
    .WithMailer(
        sp => new ResendMailer(resendApiKey),
        policy => policy.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 1,
            ShouldHandle = new PredicateBuilder().Handle<Exception>()
        }))
    .WithMailer(sp => new SmtpMailer(smtpCredentials))  // No retry policy
    .WithCompiler<ScribanCompiler>()
    .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
    .WithResolver<ViewModelTemplateResolver>()
    .WithDefaultComposer();
});
```

#### Type-based registration

You can also use type-based registration (without a factory) if the mailer type is already registered in DI:

```csharp
serviceCollection.AddScoped<SendGridMailer>(sp => new SendGridMailer(credentials));

serviceCollection.AddFacteur(x =>
{
    x.WithMailer<SendGridMailer>(
        factory: null,  // Uses DI to resolve
        policy => policy.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            ShouldHandle = new PredicateBuilder().Handle<Exception>()
        }))
    .WithCompiler<ScribanCompiler>()
    .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
    .WithResolver<ViewModelTemplateResolver>()
    .WithDefaultComposer();
});
```

#### Advanced Polly configuration

The `configurePolicy` parameter gives you full access to Polly's `ResiliencePipelineBuilder`, allowing you to configure complex retry strategies, circuit breakers, timeouts, and more:

```csharp
x.WithMailer(
    sp => new SendGridMailer(sendGridCredentials),
    policy =>
    {
        policy.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
        });
        policy.AddTimeout(TimeSpan.FromSeconds(30));
        policy.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 8
        });
    })
```

The failover mechanism uses Polly's resilience pipeline for retry policies, giving you full control over retry behavior including delays, backoff strategies, exception handling, circuit breakers, and timeouts.