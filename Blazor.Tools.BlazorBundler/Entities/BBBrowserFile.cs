using Microsoft.AspNetCore.Components.Forms;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class BBBrowserFile : IBrowserFile
    {
        private readonly IBrowserFile _browserFile;

        public BBBrowserFile(IBrowserFile browserFile)
        {
            _browserFile = browserFile;

            Name = browserFile.Name;
            LastModified = browserFile.LastModified;
            Size = browserFile.Size;
            ContentType = browserFile.ContentType;
        }

        public string Name { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            return _browserFile.OpenReadStream(maxAllowedSize, cancellationToken);
        }
    }
}
