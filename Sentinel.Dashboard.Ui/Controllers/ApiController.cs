using Sentinel.Dashboard.Ui.Model.Repositories;

namespace Sentinel.Dashboard.Ui.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    private readonly IPrometheusRepository _prometheusRepository;

    public ApiController(IPrometheusRepository prometheusRepository)
    {
        _prometheusRepository = prometheusRepository;
    }

    [HttpGet("api/deployments")]
    public ContentResult GetDeployments()
    {
        var result = _prometheusRepository.GetDeployments();

        return new ContentResult
        {
            Content = result,
            ContentType = "application/json"
        };
    }
    
    [HttpGet("api/quote")]
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
    
    [HttpGet("api/environments")]
    public ContentResult OnGetEnvironments()
    {
        var result = _prometheusRepository.GetEnvironments();
        
        return new ContentResult
        {
            Content = result,
            ContentType = "application/json"
        };
    }
    
    [HttpGet("api/alerts")]
    public ContentResult OnGetAlerts()
    {
        var result = _prometheusRepository.GetAlerts();
        
        return new ContentResult
        {
            Content = result,
            ContentType = "application/json"
        };
    }
}