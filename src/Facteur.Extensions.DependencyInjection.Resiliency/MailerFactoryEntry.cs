using System;
using Polly;

namespace Facteur.Extensions.DependencyInjection.Resiliency
{
    internal record MailerFactoryEntry(Func<IServiceProvider, IMailer> Factory, ResiliencePipeline? Policy);
}