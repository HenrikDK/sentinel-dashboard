namespace Sentinel.Dashboard.Ui.Model.Repositories;

public interface IIssuesRepository
{
    IList<Issue> GetIssues(string space, string environment, string eventType);
    IList<TimeSeriesElement> GetIssuesActivity(string space, string environment, string timeSpan, string eventType);
    
    IList<Error> GetIssueEvents(string space, string environment, Issue issue, string eventType);
    IList<TimeSeriesElement> GetIssueActivity(string space, string environment, string timeSpan, Issue issue, string eventType);
}

public class IssuesRepository : IIssuesRepository
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IHumioQueryRepository _humioQueryRepository;
    private Lazy<string> _server;
    private Lazy<string> _token;
    private TimeZoneInfo _clientZone;

    public IssuesRepository(IConfiguration configuration, IMemoryCache cache, IHumioQueryRepository humioQueryRepository)
    {
        _configuration = configuration;
        _cache = cache;
        _humioQueryRepository = humioQueryRepository;

        _clientZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        _server = new Lazy<string>(() => _configuration.GetValue("humio-api", ""));
        _token = new Lazy<string>(() => _configuration.GetValue("humio-search-token", ""));
    }

    public IList<Issue> GetIssues(string space, string environment, string eventType)
    {
        return _cache.GetOrCreate($"{space}-{environment}-{eventType}-overview", x =>
        {
            x.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
            return GetIssuesFromServer(space, environment, eventType); 
        });
    }
    
    private IList<Issue> GetIssuesFromServer(string space, string environment, string eventType)
    {
        var repository = _configuration.GetValue($"humio-repository-{space}", "");

        var query = _humioQueryRepository.GetIssuesListQuery(environment, eventType);

        var result = _server.Value.AppendPathSegment($"/api/v1/repositories/{repository}/query")
            .WithOAuthBearerToken(_token.Value)
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")
            .WithSettings(x => x.JsonSerializer = new DefaultJsonSerializer( new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            }))
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

    public IList<TimeSeriesElement> GetIssuesActivity(string space, string environment, string timeSpan, string eventType)
    {
        return _cache.GetOrCreate($"{space}-{environment}-{timeSpan}-{eventType}-chart", x =>
        {
            x.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
            return GetIssuesActivityFromServer(space, environment, timeSpan, eventType); 
        });
    }

    private IList<TimeSeriesElement> GetIssuesActivityFromServer(string space, string environment, string timeSpan, string eventType)
    {
        var repository = _configuration.GetValue($"humio-repository-{space}", "");
        
        var query = _humioQueryRepository.GetOverviewChartQuery(environment, timeSpan, eventType);
        
        var result = _server.Value.AppendPathSegment($"/api/v1/repositories/{repository}/query")
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
    
    public IList<TimeSeriesElement> GetIssueActivity(string space, string environment, string timeSpan, Issue issue, string eventType)
    {
        return _cache.GetOrCreate($"{space}-{environment}-{issue.Id}-{timeSpan}-{eventType}-minichart", x =>
        {
            x.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
            return GetIssueActivityFromServer(space, environment, timeSpan, issue, eventType);
        });
    }
    
    public IList<TimeSeriesElement> GetIssueActivityFromServer(string space, string environment, string timeSpan, Issue issue, string eventType)
    {
        var repository = _configuration.GetValue($"humio-repository-{space}", "");
        
        var query = _humioQueryRepository.GetMiniChartQuery(environment, timeSpan, issue, eventType);
        
        var result = _server.Value.AppendPathSegment($"/api/v1/repositories/{repository}/query")
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
    
    public IList<Error> GetIssueEvents(string space, string environment, Issue issue, string eventType)
    {
        return _cache.GetOrCreate($"{space}-{environment}-{issue.Id}-{eventType}-events", x =>
        {
            x.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
            return GetIssueEventsFromServer(space, environment, issue, eventType);
        });
    }
    
    private IList<Error> GetIssueEventsFromServer(string space, string environment, Issue issue, string eventType)
    {
        var repository = _configuration.GetValue($"humio-repository-{space}", "");

        var query = _humioQueryRepository.GetEventsQuery(environment, issue, eventType);
        
        var result = _server.Value.AppendPathSegment($"/api/v1/repositories/{repository}/query")
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