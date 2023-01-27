namespace Sentinel.Dashboard.Ui.Model.Repositories;

public interface IHumioRepository
{
    IList<string> GetEnvironments();

    IList<Issue> GetOverview(string environment);
    IList<TimeSeriesElement> GetActivity(string environment, string timeSpan);
    IList<TimeSeriesElement> GetIssueActivity(string environment, string timeSpan, Issue issue);

    IList<Error> GetErrorIndex(string environment, Issue issue);
}

public class HumioRepository : IHumioRepository
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private Lazy<string> _server;
    private Lazy<string> _token;
    private Lazy<string> _repository;
    private Lazy<string> _filter;
    private TimeZoneInfo _clientZone;

    public HumioRepository(IConfiguration configuration, IMemoryCache cache)
    {
        _configuration = configuration;
        _cache = cache;
        
        _clientZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        _server = new Lazy<string>(() => _configuration.GetValue("humio-api", "https://cloud.humio.com"));
        _token = new Lazy<string>(() => _configuration.GetValue("humio-token", ""));
        _repository = new Lazy<string>(() => _configuration.GetValue("humio-repository", ""));
        _filter = new Lazy<string>(() => _configuration.GetValue("namespace-filter", ""));
    }

    public IList<Issue> GetOverview(string environment)
    {
        return _cache.GetOrCreate($"{environment}-overview", x =>
        {
            x.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
            return GetOverviewFromServer(environment); 
        });
    }
    
    private IList<Issue> GetOverviewFromServer(string environment)
    {
        var query = $$"""
kubernetes.namespace="{{environment}}" |
message.Level="Error" |
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
LastSeen:=formatTime("%Y-%m-%d %H:%M:%S", field=@timestamp, timezone="Europe/Copenhagen") |
FirstSeen:=formatTime("%Y-%m-%d %H:%M:%S", field=firsttime, timezone="Europe/Copenhagen") |
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

        var result = _server.Value.AppendPathSegment($"/api/v1/repositories/{_repository.Value}/query")
            .WithOAuthBearerToken(_token.Value)
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")
            .PostJsonAsync(new
            {
                queryString = query,
                start = "30days",
                end = "now"
            })
            .ReceiveJson<IList<Issue>>().Result;
        
        result.ForEach(x => x.Id = $"{x.Service}{x.ExceptionType}{x.MessageTemplate}{x.SourceContext}".ToSha256Base36());

        var now = GetCurrentClientTime();

        result = result.Where(x => x.LastSeen > now.AddDays(-1)).ToList();
        
        return result;
    }

    private DateTime GetCurrentClientTime()
    {
        var utcTime = DateTime.UtcNow;
        
        var currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, _clientZone);

        return currentDateTime;
    }

    public IList<TimeSeriesElement> GetActivity(string environment, string timeSpan)
    {
        return _cache.GetOrCreate($"{environment}-{timeSpan}-activity", x =>
        {
            x.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
            return GetActivityFromServer(environment, timeSpan); 
        });
    }

    private IList<TimeSeriesElement> GetActivityFromServer(string environment, string timeSpan)
    {
        var span = timeSpan.EndsWith("hours") ? "15min" : "1day";
        
        var query = $"""
kubernetes.namespace = "{environment}" |
message.Level="Error" |
timechart(span="{span}", series="kubernetes.container.name")
""";

        var result = _server.Value.AppendPathSegment($"/api/v1/repositories/{_repository.Value}/query")
            .WithOAuthBearerToken(_token.Value)
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")
            .PostJsonAsync(new
            {
                queryString = query,
                start = timeSpan,
                end = "now"
            })
            .ReceiveJson<IList<TimeSeriesDto>>().Result;

        var activity = MapToTimeSeries(result);
        
        return activity;
    }
    
    public IList<TimeSeriesElement> GetIssueActivity(string environment, string timeSpan, Issue issue)
    {
        return _cache.GetOrCreate($"{environment}-{issue.Id}-{timeSpan}", x =>
        {
            x.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
            return GetIssueActivityFromServer(environment, timeSpan, issue);
        });
    }
    
    public IList<TimeSeriesElement> GetIssueActivityFromServer(string environment, string timeSpan, Issue issue)
    {
        var span = timeSpan.EndsWith("hours") ? "1hour" : "1day";
        
        var query = $$"""
kubernetes.namespace = "{{environment}}" |
message.Level="Error" |
message.MessageTemplate = "{{issue.MessageTemplate?.Replace("\"","\\\"")}}" |  
kubernetes.container.name = "{{issue.Service}}" |
message.Properties.SourceContext = "{{issue.SourceContext}}" |
message.Properties.EventId.Name = "{{issue.ExceptionType}}" or message.Properties.ExceptionDetail.Type = "{{issue.ExceptionType}}" |  
timechart(span="{{span}}", series="kubernetes.container.name")
""";

        var result = _server.Value.AppendPathSegment($"/api/v1/repositories/{_repository.Value}/query")
            .WithOAuthBearerToken(_token.Value)
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")
            .PostJsonAsync(new
            {
                queryString = query,
                start = timeSpan,
                end = "now"
            }).ReceiveJson<IList<TimeSeriesDto>>().Result;

        var activity = MapToTimeSeries(result);
        
        return activity;
    }
    
    private List<TimeSeriesElement> MapToTimeSeries(IList<TimeSeriesDto> result)
    {
        var activity = result.Select(x =>
        {
            var utcTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(x.Bucket)).UtcDateTime;
            return new TimeSeriesElement
            {
                Count = int.Parse(x.Count),

                Bucket = TimeZoneInfo.ConvertTimeFromUtc(utcTime, _clientZone),
                Name = x.Name
            };
        }).ToList();
        return activity;
    }
    
    public IList<string> GetEnvironments()
    {
        return _cache.GetOrCreate("environments", x =>
        {
            x.AbsoluteExpiration = DateTimeOffset.Now.AddDays(1);
            return GetEnvironmentsFromServer();
        });
    }

    private IList<string> GetEnvironmentsFromServer()
    {
        var query = """
kubernetes.namespace="*" |
groupby(field=[kubernetes.namespace]) | 
rename(field=kubernetes.namespace, as=Name) |
table([Name]) |
sort(Name, order=asc)
""";
        
        var result = _server.Value.AppendPathSegment($"/api/v1/repositories/{_repository.Value}/query")
            .WithOAuthBearerToken(_token.Value)
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")
            .PostJsonAsync(new
            {
                queryString = query,
                start = "30days",
                end = "now"
            }).ReceiveJson<IList<Environment>>().Result;

        if (!string.IsNullOrEmpty(_filter.Value))
        {
            var filters = _filter.Value.Replace(" ", "").Split(",");
            result = result.Where(x => filters.Any(y => x.Name.StartsWith(y))).ToList();
        }

        return result.Select(x => x.Name).ToList();
    }
    
    public IList<Error> GetErrorIndex(string environment, Issue issue)
    {
        return _cache.GetOrCreate($"{environment}-{issue.Id}-index", x =>
        {
            x.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
            return GetErrorIndexFromServer(environment, issue);
        });
    }
    
    private IList<Error> GetErrorIndexFromServer(string environment, Issue issue)
    {
        var query = $$"""
kubernetes.namespace = "{{environment}}" |
message.Level="Error" |
message.MessageTemplate = "{{issue.MessageTemplate.Replace("\"","\\\"")}}" |
kubernetes.container.name = "{{issue.Service}}" |
message.Properties.SourceContext = "{{issue.SourceContext}}" |
message.Properties.EventId.Name = "{{issue.ExceptionType}}" or message.Properties.ExceptionDetail.Type = "{{issue.ExceptionType}}" |
Created:=formatTime("%Y-%m-%d %H:%M:%S", field=@timestamp, timezone="Europe/Copenhagen") |
eventSize()| 
rename(field=_eventSize, as=Size) | 
rename(field=@id, as=Id) |
rename(field=@rawstring, as=Data) |
tail(10000) |
sort(Created, order=desc) |
table([Created, Id, Size, Data])
""";
        
        var result = _server.Value.AppendPathSegment($"/api/v1/repositories/{_repository.Value}/query")
            .WithOAuthBearerToken(_token.Value)
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")
            .PostJsonAsync(new
            {
                queryString = query,
                start = "30days",
                end = "now"
            }).ReceiveJson<IList<Error>>().Result;
        
        return result;
    }
}