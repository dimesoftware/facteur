using System;
using System.Threading.Tasks;

namespace Facteur.Extensions.DependencyInjection.Tests
{
    internal class TestMailer : IMailer
    {
        private readonly bool _shouldSucceed;
        private readonly int _id;
        private readonly int? _succeedOnAttempt;

        public TestMailer(bool shouldSucceed, int id, int? succeedOnAttempt = null)
        {
            _shouldSucceed = shouldSucceed;
            _id = id;
            _succeedOnAttempt = succeedOnAttempt;
        }

        public int CallCount { get; private set; }

        public Task SendMailAsync(EmailRequest request)
        {
            CallCount++;
            if (_succeedOnAttempt.HasValue)
            {
                if (CallCount != _succeedOnAttempt.Value)
                {
                    throw new InvalidOperationException($"Mailer {_id} intentionally failed on attempt {CallCount}.");
                }
            }
            else if (!_shouldSucceed)
            {
                throw new InvalidOperationException($"Mailer {_id} intentionally failed.");
            }

            return Task.CompletedTask;
        }

        public Task SendMailAsync(Func<IEmailComposer, Task<EmailRequest>> compose)
        {
            CallCount++;
            if (_succeedOnAttempt.HasValue)
            {
                if (CallCount != _succeedOnAttempt.Value)
                {
                    throw new InvalidOperationException($"Mailer {_id} intentionally failed on attempt {CallCount}.");
                }
            }
            else if (!_shouldSucceed)
            {
                throw new InvalidOperationException($"Mailer {_id} intentionally failed.");
            }

            return Task.CompletedTask;
        }
    }
}