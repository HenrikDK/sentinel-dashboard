using DSB.Sentinel.Dashboard.Ui.Model.Repositories;

namespace DSB.Sentinel.Dashboard.Ui.Controllers;

[ApiController]
public class HumioController : ControllerBase
{
    private readonly IIssuesRepository _issuesRepository;
    private readonly IHumioQueryRepository _humioQueryRepository;

    public HumioController(IIssuesRepository issuesRepository, 
        IHumioQueryRepository humioQueryRepository)
    {
        _issuesRepository = issuesRepository;
        _humioQueryRepository = humioQueryRepository;
    }

    [HttpGet("humio/spaces/{space}/environments/{environment}/queries/{type}/issues")]
    public ActionResult GetOverviewIssuesQuery(string space, string environment, string type, [FromQuery] string timeSpan = "24hours")
    {
        var eventType = type.StartsWith("app") ? "Error" : "Warning";
        var start = timeSpan.Replace("hours", "h").Replace("days", "d");

        var repository = _humioQueryRepository.GetRepository(space);
        
        var query = _humioQueryRepository.GetIssuesListQuery(environment, eventType);
        
        var url = $"https://cloud.humio.com/{repository}/search?query={HttpUtility.UrlEncode(query)}&start={start}&tz=Europe%2FCopenhagen";
        
        return Redirect(url);
    }
    
    [HttpGet("humio/spaces/{space}/environments/{environment}/queries/{type}/issues/chart")]
    public ActionResult GetOverviewChartQuery(string space, string environment, string type, [FromQuery] string timeSpan = "24hours")
    {
        var eventType = type.StartsWith("app") ? "Error" : "Warning";
        var start = timeSpan.Replace("hours", "h").Replace("days", "d");

        var repository = _humioQueryRepository.GetRepository(space);
        
        var query = _humioQueryRepository.GetOverviewChartQuery(environment, timeSpan, eventType);
        
        var url = $"https://cloud.humio.com/{repository}/search?query={HttpUtility.UrlEncode(query)}&start={start}&tz=Europe%2FCopenhagen";
        
        return Redirect(url);
    }    

    [HttpGet("humio/spaces/{space}/environments/{environment}/logs/{app}/events")]
    public ActionResult GetIssueEventsQuery(string space, string environment, string app, [FromQuery] string timeSpan = "7days")
    {
        var start = timeSpan.Replace("hours", "h").Replace("days", "d");

        var repository = _humioQueryRepository.GetRepository(space);
        
        var query = _humioQueryRepository.GetLogsQuery(environment, app);
        
        var url = $"https://cloud.humio.com/{repository}/search?query={HttpUtility.UrlEncode(query)}&start={start}&tz=Europe%2FCopenhagen";
        
        return Redirect(url);
    }
    
    [HttpGet("humio/spaces/{space}/environments/{environment}/queries/{type}/issues/{issueId}/events")]
    public ActionResult GetIssueEventsQuery(string space, string environment, string type, string issueId, [FromQuery] string timeSpan = "24hours")
    {
        var eventType = type.StartsWith("app") ? "Error" : "Warning";
        var start = timeSpan.Replace("hours", "h").Replace("days", "d");

        var issues = _issuesRepository.GetIssues(space, environment, eventType);
        
        var issue = issues.FirstOrDefault(x => x.Id == issueId);
        if (issue == null)
        {
            return NotFound();
        }
        
        var repository = _humioQueryRepository.GetRepository(space);
        
        var query = _humioQueryRepository.GetEventsQuery(environment, issue, eventType);
        
        var url = $"https://cloud.humio.com/{repository}/search?query={HttpUtility.UrlEncode(query)}&start={start}&tz=Europe%2FCopenhagen";
        
        return Redirect(url);
    }
    
    [HttpGet("humio/spaces/{space}/environments/{environment}/queries/{type}/issues/{issueId}/chart")]
    public ActionResult GetIssueChartQuery(string space, string environment, string type, string issueId, [FromQuery] string timeSpan = "24hours")
    {
        var eventType = type.StartsWith("app") ? "Error" : "Warning";
        var start = timeSpan.Replace("hours", "h").Replace("days", "d");

        var issues = _issuesRepository.GetIssues(space, environment, eventType);
        
        var issue = issues.FirstOrDefault(x => x.Id == issueId);
        if (issue == null)
        {
            return NotFound();
        }
        
        var repository = _humioQueryRepository.GetRepository(space);
        
        var query = _humioQueryRepository.GetMiniChartQuery(environment, timeSpan, issue, eventType);
        
        var url = $"https://cloud.humio.com/{repository}/search?query={HttpUtility.UrlEncode(query)}&start={start}&tz=Europe%2FCopenhagen";
        
        return Redirect(url);
    }
}