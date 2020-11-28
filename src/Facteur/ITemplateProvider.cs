using System.Threading.Tasks;

namespace Facteur
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITemplateProvider
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<string> GetFile(string fileName);
    }
}