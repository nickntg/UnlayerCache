using System.Collections.Generic;

namespace UnlayerCache.API.Models
{
    public class UnlayerRenderRequest
    {
        public string displayMode { get; set; }
        public Design design { get; set; }
        public Dictionary<string, string> mergeTags { get; set; }
    }
}
