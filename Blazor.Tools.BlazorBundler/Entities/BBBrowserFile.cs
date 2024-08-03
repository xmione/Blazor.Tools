using Microsoft.AspNetCore.Components.Forms;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class BBBrowserFile : IBrowserFile
    {
        public string Name { get; set; } = default!;
        public DateTimeOffset LastModified { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; } = default!;

        // This is a placeholder implementation. You'll need to provide the actual file data storage mechanism.
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            // Placeholder: Return an empty memory stream or implement actual file data handling
            return new MemoryStream(); // Return an empty stream or your stored file data
        }
    }
}
