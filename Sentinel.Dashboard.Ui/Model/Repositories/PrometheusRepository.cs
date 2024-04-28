namespace Sentinel.Dashboard.Ui.Model.Repositories;

public interface IPrometheusRepository
{
    string GetDeployments();
    string GetEnvironments();
    string GetAlerts();
}

public class PrometheusRepository : IPrometheusRepository
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private Lazy<string> _server;
    private Lazy<string> _username;
    private Lazy<string> _password;
    private readonly Lazy<string> _org;

    public PrometheusRepository(IConfiguration configuration, IMemoryCache cache)
    {
        _configuration = configuration;
        _cache = cache;

        _server = new Lazy<string>(() => _configuration.GetValue("prometheus-api", ""));
        _username = new Lazy<string>(() => _configuration.GetValue("prometheus-user", ""));
        _password = new Lazy<string>(() => _configuration.GetValue("prometheus-pass", ""));
        _org = new Lazy<string>(() => _configuration.GetValue("prometheus-org", ""));
    }
    
    public string GetDeployments()
    {
        return _cache.GetOrCreate("deployments", x =>
        {
            var deployments = GetDeploymentsFromServer();
            x.AbsoluteExpiration = DateTime.Now.AddSeconds(10);
            return deployments;
        });
    }
    
    private string GetDeploymentsFromServer()
    {
        var query = "up{aks_version!=\"\", aks_space!=\"\"}";
        
        var result = _server.Value
            .AppendPathSegment("api/v1/query")
            .WithHeader("X-Scope-OrgId", _org.Value)
            .SetQueryParam("query", query)
            .WithBasicAuth(_username.Value, _password.Value)
            .GetStringAsync().Result;

        return result;
    }

    public string GetEnvironments()
    {
        return _cache.GetOrCreate("environments", x =>
        {
            var deployments = GetEnvironmentsFromServer();
            x.AbsoluteExpiration = DateTime.Now.AddSeconds(10);
            return deployments;
        });
    }
    
    private string GetEnvironmentsFromServer()
    {
        var query = "count by (aks_space, namespace) (up{aks_version!=\"\", aks_space!=\"\"})";
        
        var result = _server.Value
            .AppendPathSegment("api/v1/query")
            .WithHeader("X-Scope-OrgId", _org.Value)
            .SetQueryParam("query", query)
            .WithBasicAuth(_username.Value, _password.Value)
            .GetStringAsync().Result;

        return result;
    }

    public string GetAlerts()
    {
        return _cache.GetOrCreate("alerts", x =>
        {
            var deployments = GetAlertsFromServer();
            x.AbsoluteExpiration = DateTime.Now.AddSeconds(10);
            return deployments;
        });
    }
    
    private string GetAlertsFromServer()
    {
        var query = "alerts{alertstate=\"firing\", env=\"prd\", aks_space!=\"\"}";
        
        var result = _server.Value
            .AppendPathSegment("api/v1/query")
            .WithHeader("X-Scope-OrgId", _org.Value)
            .SetQueryParam("query", query)
            .WithBasicAuth(_username.Value, _password.Value)
            .GetStringAsync().Result;

        return result;
    }
}