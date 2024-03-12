using System.Threading.Tasks;

namespace Facteur
{
    public class EmailComposer<T> : BaseEmailComposer<EmailRequest<T>> where T : class
    {
        private readonly IMailBodyBuilder _bodyBuilder;

        public EmailComposer(IMailBodyBuilder bodyBuilder = null)
        {
            _bodyBuilder = bodyBuilder;
        }

        public EmailComposer<T> SetModel(T model)
        {
            Request.Model = model;
            return this;
        }

        public override async Task<EmailRequest<T>> BuildAsync()
        {
            EmailRequest<T> request = base.Build();
            return _bodyBuilder == null ? request : (EmailRequest<T>)await _bodyBuilder?.BuildAsync(request);
        }
    }
}