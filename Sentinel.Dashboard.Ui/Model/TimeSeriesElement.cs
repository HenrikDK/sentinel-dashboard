namespace Sentinel.Dashboard.Ui.Model;

public class TimeSeriesElement
{
    public int Count { get; set; }
    public DateTime Bucket { get; set; }
    public string Name { get; set; } = "";
}

public class TimeSeriesDto
{
    [JsonPropertyName("_count")]
    public string Count { get; set; }
    
    [JsonPropertyName("_bucket")]
    public string Bucket { get; set; }
    
    [JsonPropertyName("kubernetes.container.name")]
    public string Name { get; set; } = "";
}