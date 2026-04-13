![](https://raw.githubusercontent.com/dimesoftware/facteur/master/assets/facteur.svg?raw=true =250)

# Facteur - Handlebars Compiler

Facteur (French for mailman) is a library for sending emails in .NET. This package provides a template compiler using the Handlebars template engine.

Check out the **[docs](https://dimesoftware.github.io/facteur/)** for more info.

## Installation

`dotnet add package Facteur.Compilers.Handlebars`

## Usage

```csharp
serviceCollection.AddFacteur(x =>
{
    x.WithMailer(y => new SmtpMailer(credentials, y.GetService<IEmailComposer>()))
    .WithCompiler<HandlebarsCompiler>()
    .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".hbs"))
    .WithResolver<ViewModelTemplateResolver>()
    .WithDefaultComposer();
});
```

Templates use the Handlebars syntax:

```html
<p>Hi {{name}},</p>
```
