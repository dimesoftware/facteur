using System;

namespace Facteur.TemplateProviders.IO
{
    public class AppDirectoryTemplateProvider : FileTemplateProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="extensionName"></param>
        public AppDirectoryTemplateProvider(string relativePath, string extensionName) 
            : base(AppDomain.CurrentDomain.BaseDirectory, relativePath, extensionName)
        {
        }
    }
}