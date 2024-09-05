namespace Blazor.Tools.BlazorBundler.SessionManagement.Interfaces
{
    public interface ISessionTable
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public byte[] Value { get; set; }
        public DateTimeOffset? ExpiresAtTime { get; set; }
        public long? SlidingExpirationInSeconds { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
    }
}
