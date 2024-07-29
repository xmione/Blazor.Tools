using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Blazor.Tools.BlazorBundler.Interfaces;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class SessionTable : ISessionTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string? Name { get; set; }
        public byte[] Value { get; set; } = default!;
        public DateTimeOffset? ExpiresAtTime { get; set; }
        public long? SlidingExpirationInSeconds { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
    }
}
