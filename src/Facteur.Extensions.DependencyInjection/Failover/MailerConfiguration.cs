using System;
using Polly;

namespace Facteur.Extensions.DependencyInjection
{
    internal record MailerConfiguration(Func<IServiceProvider, IMailer> Factory, ResiliencePipeline? Policy);
}