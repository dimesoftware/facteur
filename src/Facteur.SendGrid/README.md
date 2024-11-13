![Facteur Logo](https://raw.githubusercontent.com/dimesoftware/facteur/master/assets/facteur.svg?raw=true)

# Facteur

Facteur (French for mailman) is a library for sending emails in .NET. Its modular approach allows you to assemble a mail system rather than having to use a take-it-or-leave it service.

Check out the **[📚 docs »](https://dimesoftware.github.io/facteur/)** for more info.

## Quick start

### 1. Install packages

In this quick start, we choose to go for the following packages:

```cmd
dotnet add package Facteur
dotnet add package Facteur.Extensions.DependencyInjection
dotnet add package Facteur.Smtp
dotnet add package Facteur.Compilers.Scriban
dotnet add package Facteur.TemplateProviders.IO
dotnet add package Facteur.Resolvers.ViewModel
```

### 2. Create mailing composition

In the Startup class, add the following:

```csharp

// We're using good ol' Gmail for this one
SmtpCredentials credentials = new("smtp.gmail.com", "587", "false", "true", "myuser@gmail.com", "mypassword");

serviceCollection.AddFacteur(x =>
{
    x.WithMailer(y => new SmtpMailer(credentials, y.GetService<IEmailComposer>()))
    .WithCompiler<ScribanCompiler>()
    .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".sbnhtml"))
    .WithResolver<ViewModelTemplateResolver>()
    .WithDefaultComposer();
});
```

### 3. Mail template

In the project, add a `Templates` directory and add a file named `Welcome.sbnhtml`:

```sbhtml
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sample SBNHTML</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
        }
        h1 {
            color: #4CAF50;
        }
        .content {
            padding: 10px;
            background-color: #f9f9f9;
            border: 1px solid #ddd;
        }
    </style>
</head>
<body>
    <h1>Welcome, {{Name}}</h1>
    <div class="content">

        <p>Hello {{Name}},</p>
        <p>Thank you for joining us! Here are your account details:</p>
        <ul>
            <li>Email: {{Email}}</li>
            <li>Join Date: {{JoinDate}}</li>
        </ul>
        <p>We hope you enjoy your experience!</p>
    </div>

</body>
</html>
```

Set the `Build Action` to 'None' and set `Copy to Output Directory` to 'Copy Always'.

Next, add a class named `WelcomeMailModel` and add the properties that are used in the scriban file:

```csharp
public class WelcomeMailModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime JoinDate { get; set; }
}
```

### 4. Send mail

Add to your code a constructor parameter of type `IMailer`, and invoke the mailer:

```csharp
await mailer.SendMailAsync(x => x      
    .Subject("Welcome to the company!")
    .From("info@yourdomain.com")
    .To("john.doe@yourdomain.com")  
    .BuildAsync(new WelcomeMailModel { Name = "John Doe", Email = "john.doe@yourdomain.com", JoinDate = DateTime.Now }));
```

And an email should be underway!

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

Lastly and obviously, there are the various mail services, also known as **endpoints** in Facteur. emails can be sent with good old SMTP, Microsoft Graph API, SendGrid, etc.

## Packages

### Base library

Use the package manager NuGet to install the **base library** of Facteur:

`dotnet add package Facteur`

### Endpoints

Next it is up to you to decide which **endpoint** you want to use:

| Service             | Command                               |
| ------------------- | ------------------------------------- |
| Microsoft Graph API | `dotnet add package Facteur.MsGraph`  |
| SMTP                | `dotnet add package Facteur.Smtp`     |
| SendGrid            | `dotnet add package Facteur.SendGrid` |

### Compilers

Next, you should decide which **compiler** to use to generate the body of your email. The following packages are available:

| Resolvers | Command                                        |
| --------- | ---------------------------------------------- |
| Scriban   | `dotnet add package Facteur.Compilers.Scriban` |

### Template providers

You also have a choice in the **template providers**. Templates can be stored on a regular file drive but it might as well be stored on a blob on Azure.

| Providers | Command                                           |
| --------- | ------------------------------------------------- |
| IO        | `dotnet add package Facteur.TemplateProviders.IO` |

### Resolvers

The **resolvers** are the glue between the storage of templates and the runtime. Resolvers enable you to map templates to models.

| Resolvers | Command                                          |
| --------- | ------------------------------------------------ |
| View      | `dotnet add package Facteur.Resolvers.ViewModel` |

### Ancillary packages

Finally, there are some ancillary packages:

| Purpose      | Command                                                     |
| ------------ | ----------------------------------------------------------- |
| .NET Core DI | `dotnet add package Facteur.Extensions.DependencyInjection` |

## Initialization

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