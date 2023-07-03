using System.Web;
using Sentinel.Dashboard.Ui.Model.Repositories;

namespace Sentinel.Dashboard.Ui.Controllers;

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

    [HttpGet("humio/spaces/{space}/environments/{environment}/queries/{type}")]
    public ActionResult GetOverviewQuery(string space, string environment, string type, [FromQuery] string timeSpan = "24hours")
    {
        var eventType = type.StartsWith("app") ? "Error" : "Warning";
        var start = timeSpan.Replace("hours", "h").Replace("days", "d");

        var repository = _humioQueryRepository.GetRepository(space);
        var query = "";
        
        if (type is "app-issues" or "data-issues")
        {
            query = _humioQueryRepository.GetIssuesListQuery(environment, eventType);
        }

        if (type is "app-chart" or "data-chart")
        {
            query = _humioQueryRepository.GetOverviewChartQuery(environment, timeSpan, eventType);
        }
        
        var url = $"https://cloud.humio.com/{repository}/search?query={HttpUtility.UrlEncode(query)}&start={start}&tz=Europe%2FCopenhagen";
        
        return Redirect(url);
    }
    
    [HttpGet("humio/spaces/{space}/environments/{environment}/issues/{issueId}/queries/{type}")]
    public ActionResult GetIssueQuery(string space, string environment, string type, string issueId, [FromQuery] string timeSpan = "24hours")
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
        var query = "";

        if (type is "app-events" or "data-events")
        {
            query = _humioQueryRepository.GetEventsQuery(environment, issue, eventType);
        }

        if (type is "app-minichart" or "data-minichart")
        {
            query = _humioQueryRepository.GetMiniChartQuery(environment, timeSpan, issue, eventType);
        }
        
        var url = $"https://cloud.humio.com/{repository}/search?query={HttpUtility.UrlEncode(query)}&start={start}&tz=Europe%2FCopenhagen";
        
        return Redirect(url);
    }
}