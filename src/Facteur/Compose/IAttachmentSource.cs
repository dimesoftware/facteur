using System.Threading.Tasks;

namespace Facteur
{
    public interface IAttachmentSource
    {
        Task<Attachment> Fetch(string path);
    }
}