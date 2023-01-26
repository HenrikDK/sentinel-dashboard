using Sentinel.Dashboard.Ui.Model;
using Sentinel.Dashboard.Ui.Model.Repositories;

namespace Sentinel.Dashboard.Ui.Pages;

public class IndexModel : PageModel
{
    private readonly IHumioRepository _humioRepository;

    public IndexModel(IHumioRepository humioRepository)
    {
        _humioRepository = humioRepository;
    }

    public void OnGet()
    {

    }
    
    public JsonResult OnGetEnvironments()
    {
        var all = _humioRepository.GetEnvironments();
        return new JsonResult(all);
    }
    
    public JsonResult OnGetOverviewActivity(string environment, string timeSpan = "24hours")
    {
        var items = _humioRepository.GetActivity(environment, timeSpan);

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

    public JsonResult OnGetIssues(string environment)
    {
        var issues = _humioRepository.GetOverview(environment);
        return new JsonResult(issues);
    }
    
    public JsonResult OnGetIssueActivity(string environment, string issueId, string timeSpan = "24hours")
    {
        var issues = _humioRepository.GetOverview(environment);
        var issue = issues.FirstOrDefault(x => x.Id == issueId);
        if (issue == null)
        {
            var empty = new { };
            return new JsonResult(empty);
        }
        
        var items = _humioRepository.GetIssueActivity(environment, timeSpan, issue);

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
    
    public JsonResult OnGetErrors(string environment, string issueId)
    {
        var issues = _humioRepository.GetOverview(environment);
        var issue = issues.FirstOrDefault(x => x.Id == issueId);
        if (issue == null)
        {
            NotFound(new List<Error>());
        }

        var errorIndex = _humioRepository.GetErrorIndex(environment, issue);
        
        return new JsonResult(errorIndex);
    }
    
    public ContentResult OnGetQuote()
    {
        var result = "http://loremricksum.com/api/?paragraphs=1&quotes=1"
            .GetStringAsync().Result;

        return new ContentResult
        {
            Content = result,
            ContentType = "application/json"
        };
    }
}
