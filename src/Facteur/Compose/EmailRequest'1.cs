namespace Facteur
{
    public class EmailRequest<T> : EmailRequest
    {
        public T Model { get; set; }
    }
}
