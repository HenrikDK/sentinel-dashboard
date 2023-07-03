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
    private Lazy<string> _server;
    private Lazy<string> _username;
    private Lazy<string> _password;
    private Lazy<string> _org;

    public PrometheusRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        
        _server = new Lazy<string>(() => _configuration.GetValue("prometheus-api", "https://mimir.azure.dsb.dk/prometheus"));
        _username = new Lazy<string>(() => _configuration.GetValue("prometheus-user", ""));
        _password = new Lazy<string>(() => _configuration.GetValue("prometheus-pass", ""));
        _org = new Lazy<string>(() => _configuration.GetValue("prometheus-org", ""));
    }
    
    public string GetEnvironments()
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

}