using System.Diagnostics.CodeAnalysis;

namespace Facteur.TemplateProviders.IO.Tests
{
    [ExcludeFromCodeCoverage]
    public class FaultyTemplateProvider : FileTemplateProvider
    {
        public FaultyTemplateProvider() : base(@"Z:\", "DoesNotExist", ".cshtml")
        {
        }
    }
}