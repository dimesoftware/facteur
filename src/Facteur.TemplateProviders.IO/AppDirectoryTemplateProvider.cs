using System;

namespace Facteur.TemplateProviders.IO
{
    public class AppDirectoryTemplateProvider : FileTemplateProvider
    {
        public AppDirectoryTemplateProvider() : base(AppDomain.CurrentDomain.BaseDirectory)
        {
        }
    }
}