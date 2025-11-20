using System.IO;
using System.Threading.Tasks;

namespace Facteur.Attachments.IO
{
    public class FileAttachment : IAttachmentSource
    {
        public async Task<Attachment> Fetch(string path)
            => new()
            {
                ContentBytes = await File.ReadAllBytesAsync(path),
                Name = Path.GetFileName(path)
            };
    }
}