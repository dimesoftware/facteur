using System.IO;
using System.Threading.Tasks;

namespace Facteur.Attachments.IO
{
    public class FileAttachment : IAttachmentSource
    {
        public async Task<Attachment> Fetch(string path)
        {
#if NET461
            return new()
            {
                ContentBytes = File.ReadAllBytes(path),
                Name = Path.GetFileName(path)
            };
#else
            return new()
            {
                ContentBytes = await File.ReadAllBytesAsync(path),
                Name = Path.GetFileName(path)
            };
#endif
        }
    }
}