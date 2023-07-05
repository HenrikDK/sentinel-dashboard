using Sentinel.Dashboard.Ui.Model;
using Sentinel.Dashboard.Ui.Model.Repositories;

namespace Sentinel.Dashboard.Ui.Controllers;

[ApiController]
public class IssuesController : ControllerBase
{
    private readonly IIssuesRepository _issuesRepository;

    public IssuesController(IIssuesRepository issuesRepository)
    {
        _issuesRepository = issuesRepository;
    }

    [HttpGet("api/spaces/{space}/environments/{environment}/{type}/issues")]
    public ActionResult<List<Issue>> GetAppIssues(string space, string environment, string type)
    {
        var eventType = type.StartsWith("app") ? "Error" : "Warning";

        var issues = _issuesRepository.GetIssues(space, environment, eventType);
        
        return Ok(issues);
    }

    [HttpGet("api/spaces/{space}/environments/{environment}/{type}/issues/graph")]
    public JsonResult GetAppIssuesGraph(string space, string environment, string type, [FromQuery] string timeSpan = "24hours")
    {
        var eventType = type.StartsWith("app") ? "Error" : "Warning";

        var items = _issuesRepository.GetIssuesActivity(space, environment, timeSpan, eventType);

        var times = items.Select(x => x.Bucket).Distinct().ToList();
        times.Sort();
                
        var categories = times.Select(x => x.ToString("HH:mm")).ToList();

        var grouped = items.ToLookup(x => x.Name);

        var series = grouped.Select(x => new
        {
            name = x.Key,
            data = x.OrderBy(y => y.Bucket).Select(y => y.Count).ToList(),
            type = "bar",
            stack = "x"
        }).ToList();
        series = series.OrderBy(x => x.name).ToList();

        return new JsonResult(new { categories, series });
    }
    
    [HttpGet("api/spaces/{space}/environments/{environment}/{type}/issues/{issueId}")]
    public JsonResult GetAppIssueDetail(string space, string environment, string type, string issueId, [FromQuery] string timeSpan = "24hours")
    {
        var eventType = type.StartsWith("app") ? "Error" : "Warning";

        var issues = _issuesRepository.GetIssues(space, environment, eventType);
        var issue = issues.FirstOrDefault(x => x.Id == issueId);
        if (issue == null)
        {
            var empty = new { };
            return new JsonResult(empty);
        }
        
        var items = _issuesRepository.GetIssueActivity(space, environment, timeSpan, issue, eventType);

        var times = items.Select(x => x.Bucket).Distinct().ToList();
            times.Sort();
                
        var categories = times.Select(x => x.ToMiniChart(timeSpan)).ToList();

        var sorted = items.OrderBy(y => y.Bucket).ToList();
        
        var series = sorted.Select(y => y.Count).ToList();

        var indicies = sorted.Where(x => x.Count > 0).Select(x => sorted.IndexOf(x)).ToList();

        var timescale = timeSpan == "24hours" ? DateTime.Now.AddDays(-1) : DateTime.Now.AddDays(-30);
        
        var lastSeenIndex = -1;
        if (issue.LastSeen > timescale)
        {
            lastSeenIndex = indicies.Any() ? indicies.Max() : -1;
        }

        var firstSeenIndex = -1;
        if (issue.FirstSeen > timescale)
        {
            firstSeenIndex = indicies.Any() ? indicies.Min() : -1;
        }

        return new JsonResult(new { categories, series, issue, firstSeenIndex, lastSeenIndex });
    }
    
    [HttpGet("api/spaces/{space}/environments/{environment}/{type}/issues/{issueId}/events")]
    public ActionResult<List<Error>> GetAppIssueEvents(string space, string environment, string type, string issueId)
    {
        var eventType = type.StartsWith("app") ? "Error" : "Warning";

        var issues = _issuesRepository.GetIssues(space, environment, eventType);
        var issue = issues.FirstOrDefault(x => x.Id == issueId);
        if (issue == null)
        {
            NotFound(new List<Error>());
        }

        var errorIndex = _issuesRepository.GetIssueEvents(space, environment, issue, eventType);
        
        return new JsonResult(errorIndex);
    }
}