namespace Facteur
{
    /// <summary>
    /// Represents a type that can find the name of a file using the data provided by the generic types.
    /// </summary>
    public interface ITemplateResolver
    {
        string Resolve<T>();

        string Resolve<T>(T model);
    }
}