using Newtonsoft.Json;

namespace Sentinel.Dashboard.Ui.Model;

public class TimeSeriesElement
{
    public int Count { get; set; }
    public DateTime Bucket { get; set; }
    public string Name { get; set; } = "";
}

public class TimeSeriesDto
{
    [JsonProperty("_count")]
    public string Count { get; set; }
    
    [JsonProperty("_bucket")]
    public string Bucket { get; set; }
    
    [JsonProperty("kubernetes.container.name")]
    public string Name { get; set; } = "";
}