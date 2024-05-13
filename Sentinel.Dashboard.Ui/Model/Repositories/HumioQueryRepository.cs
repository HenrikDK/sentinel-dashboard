namespace Sentinel.Dashboard.Ui.Model.Repositories;

public interface IHumioQueryRepository
{
    string GetRepository(string space);
    string GetIssuesListQuery(string environment, string eventType);
    string GetOverviewChartQuery(string environment, string timeSpan, string eventType);
    string GetEventsQuery(string environment, Issue issue, string eventType);
    string GetMiniChartQuery(string environment, string timeSpan, Issue issue, string eventType);
    string GetLogsQuery(string environment, string app);
}

public class HumioQueryRepository : IHumioQueryRepository
{
    private readonly IConfiguration _configuration;

    public HumioQueryRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetRepository(string space)
    {
        return _configuration.GetValue($"humio-repository-{space}", "");
    }

    public string GetIssuesListQuery(string environment, string eventType)
    {
        var query = $$"""
kubernetes.namespace="{{environment}}" |
message.Level="{{eventType}}" |
case {
    n:=(now()-@timestamp)/1000/60/60/24 |
    n < 1 | today:=1;
    true | today:=0;
} |
case {
    message.Properties.EventId.Name = * | exception := message.Properties.EventId.Name;
    message.Properties.ExceptionDetail.Type = * | exception := message.Properties.ExceptionDetail.Type;
    true | exception:="";
} |
groupby(field=[kubernetes.container.name, message.MessageTemplate, message.Properties.SourceContext, exception], 
function=[count(), selectLast(container.image.name), selectLast(message.RenderedMessage), min(@timestamp, as=firsttime), max(@timestamp, as=@timestamp), sum(today, as=Events24Hours)]) | 
LastSeen:=formatTime("%Y-%m-%dT%H:%M:%S.%LZ", field=@timestamp, timezone="UTC") |
FirstSeen:=formatTime("%Y-%m-%dT%H:%M:%S.%LZ", field=firsttime, timezone="UTC") |
rename(field=kubernetes.container.name, as=Service)|
rename(field=_count, as=Events30Days)|
rename(field=exception, as=ExceptionType)|
rename(field=message.MessageTemplate, as=MessageTemplate)|
rename(field=message.RenderedMessage, as=RenderedMessage)|
rename(field=message.Properties.SourceContext, as=SourceContext)|
rename(field=container.image.name, as=ContainerImage)|
tail(10000)|
sort(LastSeen, order=desc)|
table([LastSeen, FirstSeen, Events30Days, Events24Hours, Service, ContainerImage, RenderedMessage, MessageTemplate, SourceContext, ExceptionType])
""";
        return query;
    }
    
    public string GetOverviewChartQuery(string environment, string timeSpan, string eventType)
    {
        var span = timeSpan.EndsWith("hours") ? "15min" : "1day";
        
        var query = $"""
kubernetes.namespace = "{environment}" |
message.Level="{eventType}" |
timechart(span="{span}", series="kubernetes.container.name")
""";
        return query;
    }

    public string GetMiniChartQuery(string environment, string timeSpan, Issue issue, string eventType)
    {
        var span = timeSpan.EndsWith("hours") ? "1hour" : "1day";

        var query = $"""
kubernetes.namespace = "{environment}" |
message.Level="{eventType}" |
kubernetes.container.name = "{issue.Service}" |
""";

        if (!string.IsNullOrEmpty(issue.MessageTemplate))
        {
            query += Environment.NewLine;
            query += $"""message.MessageTemplate = "{issue.MessageTemplate?.Replace("\"","\\\"")}" |""";
        }

        if (!string.IsNullOrEmpty(issue.ExceptionType))
        {
            query += Environment.NewLine;
            query += $"""message.Properties.EventId.Name = "{issue.ExceptionType}" or message.Properties.ExceptionDetail.Type = "{issue.ExceptionType}" |""";
        }

        if (!string.IsNullOrEmpty(issue.SourceContext))
        {
            query += Environment.NewLine;
            query += $"""message.Properties.SourceContext = "{issue.SourceContext}" |""";
        }
        
        query += Environment.NewLine;
        query += $"""timechart(span="{span}", series="kubernetes.container.name")""";
        
        return query;
    }

    public string GetLogsQuery(string environment, string app)
    {
        var query = $"""
kubernetes.namespace="{environment}" |
kubernetes.pod.name="{app}-*"
""";
        return query;
    }

    public string GetEventsQuery(string environment, Issue issue, string eventType)
    {
        var query = $"""
kubernetes.namespace = "{environment}" |
message.Level="{eventType}" |
kubernetes.container.name = "{issue.Service}" |
""";
        if (!string.IsNullOrEmpty(issue.MessageTemplate))
        {
            query += Environment.NewLine;
            query += $"""message.MessageTemplate = "{issue.MessageTemplate?.Replace("\"","\\\"")}" |""";
        }

        if (!string.IsNullOrEmpty(issue.ExceptionType))
        {
            query += Environment.NewLine;
            query += $"""message.Properties.EventId.Name = "{issue.ExceptionType}" or message.Properties.ExceptionDetail.Type = "{issue.ExceptionType}" |""";
        }

        if (!string.IsNullOrEmpty(issue.SourceContext))
        {
            query += Environment.NewLine;
            query += $"""message.Properties.SourceContext = "{issue.SourceContext}" |""";
        }

        query += Environment.NewLine;
        query += """
Created:=formatTime("%Y-%m-%d %H:%M:%S", field=@timestamp, timezone="Europe/Copenhagen") | 
eventSize() | 
rename(field=_eventSize, as=Size) | 
rename(field=@id, as=Id) |
rename(field=@rawstring, as=Data) |
tail(10000) |
sort(Created, order=desc) |
table([Created, Id, Size, Data])
""";

        return query;
    }
}