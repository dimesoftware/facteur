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
            BasePath = basePath;
            if (!Directory.Exists(BasePath))
                throw new DirectoryNotFoundException();
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

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public Task<string> GetFile(string fileName)
            => GetFile(null, fileName);

        /// <summary>
        /// Checks if the file exists
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public bool Exists(string relativePath, string fileName)
        {
            // Create full path
            string fullPath = Path.Combine(BasePath, relativePath, fileName);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Checks if the file exists
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Exists(string fileName)
        {
            // Create full path
            string fullPath = Path.Combine(BasePath, fileName);
            return File.Exists(fullPath);
        }
    }
}