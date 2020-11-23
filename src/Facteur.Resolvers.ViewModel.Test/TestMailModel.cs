namespace Facteur.Resolvers.ViewModel.Tests
{
    public class BaseModel
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }

    public class TestMailModel : BaseModel
    {
    }

    public class TestViewModel : BaseModel
    {
    }
}