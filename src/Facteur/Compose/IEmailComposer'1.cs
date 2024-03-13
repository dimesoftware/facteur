namespace Facteur
{
    public interface IEmailComposer<T> : IEmailComposer
    {
        IEmailComposer<T> SetModel(T model);
    }
}