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
        protected FileTemplateProvider(string basePath)
        {
            if (!Directory.Exists(basePath))
                throw new DirectoryNotFoundException();

            BasePath = basePath;
        }

        protected string BasePath { get; set; }

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="fileName">The full path.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public virtual Task<string> GetFile(string relativePath, string fileName)
        {
            // Create full path
            string fullPath = relativePath != null ? Path.Combine(BasePath, relativePath) : BasePath;

            // Get file name from full path and its subdirectories
            IEnumerable<string> files = Directory.GetFiles(fullPath, fileName, SearchOption.AllDirectories);
            return files.Count() == 1
                ? Task.Run(() => File.ReadAllText(files.FirstOrDefault()))
                : throw new FileNotFoundException();
        }
    }
}