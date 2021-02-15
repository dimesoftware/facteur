<p align="center"><img src="assets/letter.svg?raw=true" width="350" alt="Logo"></p>

<h1 align="center"> Facteur </h1> 

<p align="center">
<img src="https://dev.azure.com/dimenicsbe/Utilities/_apis/build/status/dimenics.facteur?branchName=master" />
<img src="https://img.shields.io/azure-devops/coverage/dimenicsbe/utilities/177" />
<img src="https://img.shields.io/badge/License-MIT-blue.svg" />
<img src="https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square" />
<a href="https://github.com/dimenics/facteur/discussions">
  <img src="https://img.shields.io/badge/chat-discussions-green">
</a>
<a href="https://gitter.im/facteur-dotnet/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=body_badge">
  <img src="https://badges.gitter.im/facteur-dotnet/community.svg">
</a>
<img src="https://img.shields.io/nuget/v/facteur" />
</p>

Facteur (French for mailman) is a library for sending e-mails in .NET. Its modular approach allows you to assemble a mail system rather than using a take-it-or-leave it service.

Check out the **[docs »](https://dimenics.github.io/facteur/)** for more info.

## About the project

The entire premise of this project is to provide a flexible and modular mailing kit. Applications should not be bound by one specific mailing service. If you get blacklisted by a mailing service or if the performance is unacceptable, you should be able to swap providers without modifying a single line of code. 

This is why we created facteur. The desire to create a flexible and vendor-independent framework is clearly reflected in the architecture.

There are a few moving parts:

- Composers
- Compilers
- Resolvers
- Template providers
- Endpoints

**Composers** enable you to create an e-mail request, which contains the e-mail variables like subject, body and the e-mail addresses to send the mail to.

**Compilers** are a part of the e-mail composition in that it allows to fetch a template and populate the e-mail body with data from a custom view model. 

The templates can be stored anywhere. By default they are stored in the folder where the application is hosted but it can also be retrieved from an Azure blob, FTP drive, etc. Using **template providers** and **resolvers**, you can write your own logic to fetch the right template for the job.

Lastly and obviously, there are the various mail services, also known as **endpoints** in Facteur. E-mails can be sent with good old SMTP, Microsoft Graph API, SendGrid, etc.

## Installation

Use the package manager NuGet to install the base library of Facteur:

`dotnet add package Facteur`

Next it is up to you to decide which *endpoint* you want to use:

| Service             | Command                               |
| ------------------- | ------------------------------------- |
| Microsoft Graph API | `dotnet add package Facteur.MsGraph`  |
| SMTP                | `dotnet add package Facteur.Smtp`     |
| SendGrid            | `dotnet add package Facteur.SendGrid` |

Next, you should decide which *compiler* to use to generate the body of your e-mail. The following packages are available:

| Resolvers   | Command                                        |
| ----------- | ---------------------------------------------- |
| RazorEngine | `dotnet add package Facteur.Compilers.Razor`   |
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

| Purpose      | Command                                                     |
| ------------ | ----------------------------------------------------------- |
| .NET Core DI | `dotnet add package Facteur.Extensions.DependencyInjection` |

## Usage

The power of this project is to create a dynamic mail body as you can populate any template with any type of data. This is when the compilers, providers and resolvers come in. They can be produced using the `MailBodyBuilder` class, which orchestrates the process of retrieving and populating the template. It is ultimately up to the instance of the `IMailer` to actually send the e-mail.

``` csharp
public async Task SendConfirmationMail(string customerMail, string customerName)
{
  EmailComposer<TestMailModel> composer = new EmailComposer<TestMailModel>();
  EmailRequest<TestMailModel> request = composer
      .SetModel(new TestMailModel { Email = customerMail, Name = customerMail })
      .SetSubject("Hello world")
      .SetFrom("info@facteur.com")
      .SetTo("guy.gadbois@facteur.com")
      .SetCc("jacques.clouseau@facteur.com")
      .SetBcc("charles.dreyfus@facteur.com")
      .Build();

  IMailBodyBuilder builder = new MailBodyBuilder(
   new ScribanCompiler(),
   new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
   new ViewModelTemplateResolver());

  EmailRequest populatedRequest = await builder.BuildAsync(request);

  SmtpCredentials credentials = new("smtp.gmail.com", "587", "false", "true", "myuser@gmail.com", "mypassword");
  IMailer mailer = new SmtpMailer(credentials);
  await mailer.SendMailAsync(populatedRequest);
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

The `IMailBodyBuilder` brings everything together and generates a populated mail body. Then it's up to the `ÌMailer` to merely send the mail.

With .NET's dependency injection, hooking up the mailer is as simple as adding one line in the Startup class:

```csharp
services.AddMailer<SmtpMailer, ScribanCompiler, AppDirectoryTemplateProvider, ViewModelTemplateResolver>(
  mailerFactory: x => new SmtpMailer(credentials),
  templateProviderFactory: x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml")
);
```

## Contributing

Pull requests are welcome. Please check out the contribution and code of conduct guidelines.