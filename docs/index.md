<img align="center" src="assets/images/facteur.svg">

<p align="center">
<img src="https://dev.azure.com/dimesoftware/Utilities/_apis/build/status/dimenics.facteur?branchName=master" />
<img src="https://img.shields.io/nuget/v/facteur" />
<img src="https://img.shields.io/azure-devops/coverage/dimesoftware/utilities/177" />
<img src="https://img.shields.io/badge/License-MIT-blue.svg" />
<img src="https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square" />
<a href="https://github.com/dimesoftware/facteur/discussions">
  <img src="https://img.shields.io/badge/chat-discussions-green">
</a>
<a href="https://gitter.im/facteur-dotnet/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=body_badge">
  <img src="https://badges.gitter.im/facteur-dotnet/community.svg">
</a>
</p>

## About the project

The entire premise of this project is to provide a **flexible** and **modular** **mailing** and **templating** kit. Applications should not be bound by one specific mailing service; like when you get blacklisted by a mailing service or when the performance is unacceptable, you should be able to swap providers without having to modify a single line of code. 

This is why we created Facteur. The desire to create a flexible and vendor-independent framework is clearly reflected in the library's architecture.

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

Next it is up to you to decide which _endpoint_ you want to use:

| Service             | Command                               |
| ------------------- | ------------------------------------- |
| Microsoft Graph API | `dotnet add package Facteur.MsGraph`  |
| SMTP                | `dotnet add package Facteur.Smtp`     |
| SendGrid            | `dotnet add package Facteur.SendGrid` |

Next, you should decide which _compiler_ to use to generate the body of your e-mail. The following packages are available:

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

| Purpose      | Command                                                     |
| ------------ | ----------------------------------------------------------- |
| .NET Core DI | `dotnet add package Facteur.Extensions.DependencyInjection` |

## Usage

### Typical example

The power of this project is to create a dynamic mail body as you can populate any template with any type of data. This is when the compilers, providers and resolvers come in. They can be produced using the `MailBodyBuilder` class, which orchestrates the process of retrieving and populating the template. It is ultimately up to the instance of the `IMailer` to actually send the e-mail.

```csharp
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

  EmailRequest populatedRequest = await builder.BuildAsync(request);

  SmtpCredentials credentials = new("smtp.gmail.com", "587", "false", "true", "myuser@gmail.com", "mypassword");
  IMailer mailer = new SmtpMailer(credentials);
  await mailer.SendMailAsync(populatedRequest);
}
```

This particular example uses scriban templates that are stored inside the application's directory. Inside the HTML template, you will find scriban syntax:

{% raw %}
```html
<p>Hi {{name}},</p>
```
{% endraw %}

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

### Basic usage

Of course, simple use case scenarios are supported as well. You can simply drop the mail body building workflow:

```csharp
EmailComposer composer = new EmailComposer());
EmailRequest request = composer
    .SetSubject("Hello world")
    .SetBody("This is the body of the mail. No template was used int the making of this e-mail")
    .SetFrom("info@facteur.com")
    .SetTo("guy.gadbois@facteur.com")
    .SetCc("jacques.clouseau@facteur.com")
    .SetBcc("charles.dreyfus@facteur.com")
    .Build();

SmtpCredentials credentials = new("smtp.gmail.com", "587", "false", "true", "myuser@gmail.com", "mypassword");
IMailer mailer = new SmtpMailer(credentials);
await mailer.SendMailAsync(populatedRequest);
```

## Composing the e-mail body

E-mail templates have an important role in Facteur. Composers, resolvers and providers only exist to fetch the templates, map them with the business logic and populate them in the e-mail body. Let's walk through the setup from the example above:

```csharp
EmailComposer builder = new(
   new ScribanCompiler(),
   new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
   new ViewModelTemplateResolver());
```

First up the list is the **compiler**. There are many templating languages available, and Scriban is one of the more popular ones. The syntax is interspersed with static content and markup in the e-mail template. The e-mail templates themselves have to be stored somewhere and loaded into memory to populate and send the template. The **providers** take on this burden and are responsible for fetching the templates from any storage medium like blobs, FTP servers, local directories, etc. Finally, we need to instruct the library which template to retrieve. **Resolvers** provide a dynamic and generic way to find the right template for the e-mail, and the possibilities are endless. In this example, the ViewModelTemplateResolver type is used, which is a very simple mechanism that maps the name of the type to the name of the file. 

Let's say we want to send an e-mail to new users. We create a `NewUserViewModel` class with public properties that need to be passed into the template.

```csharp
public class NewUserViewModel
{
  public string Name {get; set; }
  public string Manager { get; set; }
  public string IntranetUri {get;set;}
}
```

With this particular setup, there should be a file named **NewUser.sbnthml** in the `Templates` directory inside the application's app directory (in Visual Studio, make sure to mark the file to be copied to the output directory). In this file, you can use simple HTML with Scriban placeholders:

{% raw %}

```html
<html>
<body>
  <h1>Welcome, {{name}}!</h1>
  <p>Static text</p>
  <p>You will be reporting to {{manager}}.</p>

  For more information, check out the <a href="{{ intraneturi }}">company's intranet</a>.
</body>
</html>
```
{%endraw%}

Finally, to send out this particular e-mail, you just need to use the `NewUserViewModel` class in the e-mail request:

```csharp
public async Task SendWelcomeMail(string newEmployeeName, string newEmployeeMail, string managerName, string intranetUri)
{
  EmailComposer composer = new(
    new ScribanCompiler(),
    new AppDirectoryTemplateProvider("Templates", ".sbnhtml"),
    new ViewModelTemplateResolver());

  EmailRequest request = await composer      
      .SetSubject("Welcome to the company!")
      .SetFrom("info@facteur.com")
      .SetTo(newEmployeeMail)
      .BuildAsync(new NewUserViewModel { Name = newEmployeeName, Manager = managerName, IntranetUri= intranetUri });
 
  SmtpCredentials credentials = new("smtp.gmail.com", "587", "false", "true", "myuser@gmail.com", "mypassword");
  IMailer mailer = new SmtpMailer(credentials);
  await mailer.SendMailAsync(request);
}
```

The library will pick up the model type, look for the template, populate it and send the mail.

If you use DI, you can just use `IMailer` and use the overload that exposes the composer:

``` csharp
public async Task SendConfirmationMail(string customerMail, string customerName)
{
  await mailer.SendMailAsync(x =>  x      
      .SetSubject("Hello world")
      .SetFrom("info@facteur.com")
      .SetTo("guy.gadbois@facteur.com")
      .SetCc("jacques.clouseau@facteur.com")
      .SetBcc("charles.dreyfus@facteur.com")
      .BuildAsync(new TestMailModel { Email = customerMail, Name = customerMail }));
}
```


## Dependency injection

With .NET's dependency injection, hooking up the mailer is as simple as adding one line in the `Startup` class:

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
```
