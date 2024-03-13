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
        /// <param name="name"></param>
        /// <returns></returns>
        Task<string> GetTemplate(string name);
    }
}