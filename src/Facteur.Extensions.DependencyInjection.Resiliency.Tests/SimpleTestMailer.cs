namespace Facteur.Extensions.DependencyInjection.Resiliency.Tests
{
    internal class SimpleTestMailer : IMailer
    {
        public int CallCount { get; private set; }

        public Task SendMailAsync(EmailRequest request)
        {
            CallCount++;
            return Task.CompletedTask;
        }

        public Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }
}

