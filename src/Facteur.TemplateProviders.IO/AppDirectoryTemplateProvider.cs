using System;

namespace Facteur.TemplateProviders.IO
{
    public class AppDirectoryTemplateProvider : FileTemplateProvider
    {
        public AppDirectoryTemplateProvider(string relativePath, string extensionName)
            : base(AppDomain.CurrentDomain.BaseDirectory, relativePath, extensionName)
        {
        }
    }
}