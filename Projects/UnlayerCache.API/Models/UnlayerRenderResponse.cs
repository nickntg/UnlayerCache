namespace UnlayerCache.API.Models
{
    public class UnlayerRenderResponse
    {
        public bool success { get; set; }
        public RenderData data { get; set; }
    }

    public class RenderData
    {
        public string html { get; set; }
    }
}
