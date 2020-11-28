namespace Facteur.TemplateProviders.IO.Tests
{
    public class FaultyTemplateProvider : FileTemplateProvider
    {
        public FaultyTemplateProvider() : base(@"Z:\DoesNotExist")
        {
        }
    }
}