![](https://raw.githubusercontent.com/dimesoftware/facteur/master/assets/facteur.svg?raw=true =250)

# Facteur.Plunk

Facteur (French for mailman) is a library for sending emails in .NET. This package provides support for sending emails using the Plunk API.

## Installation

Use the package manager NuGet to install:

```bash
dotnet add package Facteur.Plunk
```

## Usage

### Basic Usage

```csharp
using Facteur;
using Facteur.Plunk;

// Create a mailer with your Plunk API key
IMailer mailer = new PlunkMailer("your-plunk-api-key");

// Compose and send an email
EmailComposer composer = new();
EmailRequest request = composer
    .Subject("Hello world")
    .From("info@facteur.com")
    .To("recipient@example.com")
    .Body("<h1>Hello!</h1><p>This is a test email.</p>")
    .Build();

await mailer.SendMailAsync(request);
```

### Using with Dependency Injection

```csharp
using Facteur;
using Facteur.Plunk;

serviceCollection.AddFacteur(x =>
{
    x.WithMailer(y => new PlunkMailer("your-plunk-api-key", y.GetService<IEmailComposer>()))
     .WithCompiler<ScribanCompiler>()
     .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
     .WithResolver<ViewModelTemplateResolver>()
     .WithDefaultComposer();
});
```

### Using the Composer Overload

```csharp
await mailer.SendMailAsync(async composer => await composer
    .Subject("Hello world")
    .From("info@facteur.com")
    .To("recipient@example.com")
    .Body("<h1>Hello!</h1><p>This is a test email.</p>")
    .BuildAsync());
```

### Sending with Attachments

```csharp
EmailRequest request = composer
    .Subject("Email with attachment")
    .From("info@facteur.com")
    .To("recipient@example.com")
    .Body("<p>Please find the attachment.</p>")
    .Attach("document.pdf", pdfBytes)
    .Build();

await mailer.SendMailAsync(request);
```

## API Key

To use Plunk, you need to obtain an API key from the [Plunk dashboard](https://useplunk.com). The API key should be passed to the `PlunkMailer` constructor.

## Self-Hosted Plunk

If you're using a self-hosted Plunk instance, you can specify the base URL when creating the mailer:

```csharp
// For self-hosted Plunk (e.g., https://plunk.example.com)
IMailer mailer = new PlunkMailer("your-plunk-api-key", baseUrl: "https://plunk.example.com/api");

// For hosted Plunk (default)
IMailer mailer = new PlunkMailer("your-plunk-api-key");
```

According to the [Plunk API documentation](https://docs.useplunk.com/api-reference/base-url), self-hosted instances use the `/api` endpoint of your domain.

## Features

- Send transactional emails via Plunk API
- Support for HTML email bodies
- Support for multiple recipients (To, Cc, Bcc)
- Support for email attachments
- Compatible with Facteur's templating system

## Documentation

For more information about Plunk's API, visit the [Plunk documentation](https://docs.useplunk.com).

