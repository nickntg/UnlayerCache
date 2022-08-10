namespace UnlayerCache.API.Models
{
    public class UnlayerRenderResponse
    {
        public bool success { get; set; }
        public Data2 data { get; set; }
    }

    public class Data2
    {
        public Chunks chunks { get; set; }
        public string html { get; set; }
        public Amp amp { get; set; }
    }

    public class Chunks
    {
        public string css { get; set; }
        public string js { get; set; }
        public object[] tags { get; set; }
        public Font[] fonts { get; set; }
        public string body { get; set; }
    }

    public class Font
    {
        public string label { get; set; }
        public string value { get; set; }
        public string url { get; set; }
        public bool defaultFont { get; set; }
    }

    public class Amp
    {
        public bool enabled { get; set; }
        public string format { get; set; }
        public Validation validation { get; set; }
    }

    public class Validation
    {
        public string status { get; set; }
        public object[] errors { get; set; }
    }

}
