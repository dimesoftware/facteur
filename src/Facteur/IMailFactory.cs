namespace Facteur
{
    public interface IMailFactory : IMailer
    {
        IMailFactory UseCompiler(ITemplateCompiler compiler);

        IMailFactory UseProvider(ITemplateProvider provider);

        IMailFactory UseResolver(ITemplateResolver resolver);

        IMailFactory UseMailer(IMailer mailer);
    }
}