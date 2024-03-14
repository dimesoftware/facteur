using System.Diagnostics.CodeAnalysis;

namespace Facteur.Resolvers.ViewModel.Tests
{
    [ExcludeFromCodeCoverage]
    public class BaseModel
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class TestMailModel : BaseModel
    {
    }

    [ExcludeFromCodeCoverage]
    public class TestViewModel : BaseModel
    {
    }
}