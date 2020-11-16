namespace Facteur
{
    public interface ITemplateResolver
    {
        string Resolve<T>();

        string Resolve<T>(T model);
    }
}