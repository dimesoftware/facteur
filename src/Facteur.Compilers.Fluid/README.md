![](https://raw.githubusercontent.com/dimesoftware/facteur/master/assets/facteur.svg?raw=true =250)

# Facteur - Fluid Compiler

Facteur (French for mailman) is a library for sending emails in .NET. This package provides a template compiler using the Fluid (Liquid) template engine.

Check out the **[docs](https://dimesoftware.github.io/facteur/)** for more info.

## Installation

`dotnet add package Facteur.Compilers.Fluid`

## Usage

```csharp
serviceCollection.AddFacteur(x =>
{
    x.WithMailer(y => new SmtpMailer(credentials, y.GetService<IEmailComposer>()))
    .WithCompiler<FluidCompiler>()
    .WithTemplateProvider(x => new AppDirectoryTemplateProvider("Templates", ".liquid"))
    .WithResolver<ViewModelTemplateResolver>()
    .WithDefaultComposer();
});
```

Templates use the Liquid syntax:

```html
<p>Hi {{ name }},</p>
```
