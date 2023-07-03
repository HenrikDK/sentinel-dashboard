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
}