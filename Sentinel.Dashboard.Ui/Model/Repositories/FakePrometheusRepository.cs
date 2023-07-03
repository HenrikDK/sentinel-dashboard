using Newtonsoft.Json.Linq;

namespace Sentinel.Dashboard.Ui.Model.Repositories;

public class FakePrometheusRepository : IPrometheusRepository
{
    public string GetDeployments()
    {
        return "";
    }

    public string GetEnvironments()
    {
        var result = new JObject();
        result["status"] = "success";
        result["data"] = new JObject();
        result["data"]["resultType"] = "vector";
        var set = new JArray();
        result["data"]["result"] = set;

        var ns = new[] { "lobster", "planning", "demo" };
        var envs = new[] { "sit", "prd", "dev", "uat", "tst" };
        
            ns.ForEach(n => envs.ForEach(e =>
            {
                var tmp = $$"""
{
    "metric": {
        "aks_space": "{{n}}",
        "namespace": "{{n + "-" + e}}"
    },
    "value": [
    {{DateTimeOffset.Now.ToUnixTimeSeconds()}},
    "30"
    ]
}
""";
                var item = JObject.Parse(tmp);
                set.Add(item);
            }));
        
        return result.ToString();
    }

    public string GetAlerts()
    {
        return "";
    }
}