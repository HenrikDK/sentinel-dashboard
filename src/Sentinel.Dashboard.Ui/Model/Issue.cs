namespace Sentinel.Dashboard.Ui.Model;

public class Issue
{
    public string Id { get; set; }
    public string Service { get; set; }
    public string ContainerImage { get; set; }
    public string RenderedMessage { get; set; }
    public string MessageTemplate { get; set; }
    public string ExceptionType { get; set; }
    public string SourceContext { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime FirstSeen { get; set; }
    public int Events30Days { get; set; }
    public int Events24Hours { get; set; }
}
