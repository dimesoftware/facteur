using System.Threading.Tasks;

namespace Facteur
{
    public interface ITemplateProvider
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<string> GetFile(string relativePath, string fileName);

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<string> GetFile(string fileName);

        /// <summary>
        ///
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool Exists(string relativePath, string fileName);

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool Exists(string fileName);
    }
}