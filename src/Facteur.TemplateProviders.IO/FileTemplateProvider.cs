using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Facteur.TemplateProviders.IO
{
    /// <summary>
    ///
    /// </summary>
    public abstract class FileTemplateProvider : ITemplateProvider
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="relativePath"></param>
        /// <param name="extensionName"></param>
        protected FileTemplateProvider(string basePath, string relativePath, string extensionName)
        {
            if (!Directory.Exists(basePath))
                throw new DirectoryNotFoundException();

            BasePath = basePath;
            RelativePath = relativePath;
            ExtensionName = extensionName;
        }

        protected string BasePath { get; }
        protected string RelativePath { get; }
        protected string ExtensionName { get; }

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="fileName">The full path.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public virtual Task<string> GetTemplate(string fileName)
        {
            // Create full path
            string fullPath = RelativePath != null
                ? $"{Path.Combine(BasePath, RelativePath)}"
                : $"{Path.Combine(BasePath)}";

            string fullFileName = fileName + ExtensionName;

            // Get file name from full path and its subdirectories
            IEnumerable<string> files = Directory.GetFiles(fullPath, fullFileName, SearchOption.AllDirectories);
            return files.Count() == 1
                ? Task.Run(() => File.ReadAllText(files.FirstOrDefault()))
                : throw new FileNotFoundException();
        }
    }
}