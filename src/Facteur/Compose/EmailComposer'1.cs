namespace Facteur
{
    public class EmailComposer<T> : BaseEmailComposer<EmailRequest<T>>
    {
        public EmailComposer<T> SetModel(T model)
        {
            Request.Model = model;
            return this;
        }
    }
}
