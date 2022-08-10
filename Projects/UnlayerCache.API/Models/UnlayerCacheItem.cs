namespace UnlayerCache.API.Models
{
    public class UnlayerCacheItem
    {
        public string Id { get; set; }
        public long ExpiresAt { get; set; }
        public string Value { get; set; }
    }
}
