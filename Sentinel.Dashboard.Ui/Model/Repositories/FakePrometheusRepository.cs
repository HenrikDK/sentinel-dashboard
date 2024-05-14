using System.Text.Json.Nodes;

namespace Sentinel.Dashboard.Ui.Model.Repositories;

public class FakePrometheusRepository : IPrometheusRepository
{
    public string GetDeployments()
    {
        var set = new JsonArray();
        var result = new JsonObject()
        {
            ["status"] = "success",
            ["data"] = new JsonObject()
            {
                ["resultType"] = "vector",
                ["result"] = set
            }
        };
        
        var ns = new[] { "project", "planning", "demo", "sales" };

        var workloads = ns.SelectMany(GetWorkloads);
        
        workloads.ForEach(w => set.Add(w));
        
        return result.ToJsonString();
    }
    
    private IList<JsonObject> GetWorkloads(string ns)
    {
        var workloads = new List<JsonObject>();
        
        if (ns == "planning")
        {
            var envs = new[] { "sit" ,"prd", "dev", "uat", "tst" };

            var names = new[]
            {
                new { rep = "tris", name = "tris", type ="api" }, 
                new { rep = "tris", name = "tris", type ="worker" },
                new { rep = "tris", name = "tris-legacy", type ="hub" },

                new { rep = "tds3", name = "tds3", type ="ui" }, 
                new { rep = "tds3", name = "tds3", type ="worker" },
                new { rep = "tds3", name = "tds3-wayside", type ="consumer" },

                new { rep = "graffiti", name = "graffiti", type ="api" }, 
                new { rep = "graffiti", name = "graffiti", type ="ui" },
                new { rep = "travel-search", name = "travel-search", type ="api" },
            };
            
            envs.ForEach(e => names.ForEach(n =>
            {
                var w = GetWorkload(ns, e, n.rep, n.name, n.type);
                workloads.Add(w);
            }));
        }

        if (ns == "demo")
        {
            var envs = new[] { "prd", "tst" };

            var names = new[]
            {
                new { rep = "legacy-exporter", name = "std-exporter", type ="worker" }, 
                new { rep = "legacy-exporter", name = "p90-exporter", type ="worker" },
                new { rep = "legacy-exporter", name = "oracle-exporter", type ="worker" },

                new { rep = "demo-github-sync", name = "github-sync", type ="worker" }, 
                new { rep = "demo_container-registry-cleanup", name = "container-registry-cleanup", type ="worker" },
                new { rep = "demo.school-travel", name = "school-travel", type ="ui" },

                new { rep = "departure-board", name = "departure-board", type ="ui" }, 
                new { rep = "chaos-monkey", name = "chaos-monkey", type ="worker" },
                new { rep = "dev-sec-ops", name = "dev-sec-ops", type ="worker" },
            };
            
            envs.ForEach(e => names.ForEach(n =>
            {
                var w = GetWorkload(ns, e, n.rep, n.name, n.type);
                workloads.Add(w);
            }));
        }

        if (ns == "project")
        {
            var envs = new[] { "prd", "dev", "uat", "tst" };
            
            var names = new[]
            {
                new { rep = "database-replication", name = "database-replication", type ="api" }, 
                new { rep = "database-replication", name = "database-replication", type ="worker" }, 
                
                new { rep = "station", name = "station", type ="api" },
                new { rep = "station", name = "station", type ="worker" },

                new { rep = "crew-schedule", name = "crew-schedule", type ="api" }, 
                new { rep = "crew-schedule", name = "crew-schedule", type ="worker" },
                new { rep = "crew-schedule", name = "crew-schedule", type ="consumer" },

                new { rep = "train-schedule", name = "train-schedule", type ="api" }, 
                new { rep = "train-schedule", name = "train-schedule", type ="worker" },
                new { rep = "train-schedule", name = "train-schedule", type ="consumer" },

                new { rep = "material-planning", name = "material-planning", type ="api" }, 
                new { rep = "material-planning", name = "material-planning", type ="worker" },
                new { rep = "material-planning", name = "material-planning", type ="consumer" },

                new { rep = "test-service", name = "test", type ="api" }, 
                new { rep = "test-service", name = "test-keyvault", type ="api" },
                new { rep = "test-service", name = "test", type ="consumer" },

                new { rep = "example-service", name = "example", type ="api" }, 
                new { rep = "example-service", name = "example", type ="worker" },
                new { rep = "example-service", name = "example", type ="consumer" },
            };
            
            envs.ForEach(e => names.ForEach(n =>
            {
                var w = GetWorkload(ns, e, n.rep, n.name, n.type);
                workloads.Add(w);
                if (n.name == "database-replication" && n.type == "worker" && e == "tst")
                {
                    var w2 = GetWorkload(ns, e, n.rep, n.name, n.type, true);
                    workloads.Add(w2);
                }
            }));
        }

        if (ns == "sales")
        {
            var envs = new[] { "prd", "dev" };
            
            var names = new[]
            {
                new { rep = "sales-order", name = "sales-order", type ="api" },
                new { rep = "sales-order", name = "sales-order-price", type ="hub"},
                
                new { rep = "sales-inventory", name = "sales-inventory", type ="api" }
            };
            
            envs.ForEach(e => names.ForEach(n =>
            {
                var w = GetWorkload(ns, e, n.rep, n.name, n.type);
                workloads.Add(w);
                if (e == "dev" && n.rep == "sales-order")
                {
                    var b1 = GetWorkload(ns, e, n.rep, n.name, n.type, strategy: "branch", head: "feature-fox-1234-important-changes-bla-bla");
                    workloads.Add(b1);
                    var b2 = GetWorkload(ns, e, n.rep, n.name, n.type, deploying: true, strategy: "branch", head: "feature-fox-1235-other-changes-bla-bla");
                    workloads.Add(b2);
                }
                
                if (e == "dev" && n.rep == "sales-inventory")
                {
                    var p1 = GetWorkload(ns, e, n.rep, n.name, n.type, strategy: "preview", head: "feature-TRIV-123-my-special-bla-bla");
                    workloads.Add(p1);
                }
            }));
        }

        return workloads;
    }
    
    private JsonObject GetWorkload(string ns, string env, string repo, string name, string type, bool deploying = false, string head = "", string strategy = "")
    {
        var metrics = name == "example" && type == "worker" ? "0" : "1";
        var deployed = DateTimeOffset.Now.AddDays(-Random.Shared.Next(1, 7)).ToUnixTimeSeconds();
        if (deploying)
        {
            deployed = DateTimeOffset.Now.AddMinutes(-Random.Shared.Next(1, 3)).ToUnixTimeSeconds();
        }

        var tmp = new JsonObject()
        {
            ["metric"] = new JsonObject()
            {
                ["__name__"] = "up",
                ["aks_deployed"] = $"{deployed}",
                ["aks_environment"] = env,
                ["aks_name"] = $"{name}-{type}",
                ["aks_space"] = $"{ns}",
                ["aks_systemid"] = "no",
                ["aks_type"] = $"{type}",
                ["aks_version"] = $"{DateTime.Now.AddDays(-Random.Shared.Next(1, 7)):yyyyMMdd}-{Random.Shared.Next(1, 999)}-{RandomString(7)}",
                ["app"] = $"{name}-{type}-{env}",
                ["github_branch"] = "main",
                ["github_org"] = "my-org",
                ["github_repository"] = repo,
                ["github_user"] = "HenrikDK",
                ["namespace"] = $"{ns}-{env}",
                ["pod"] = $"{name}-{type}-{RandomString(7)}-{RandomString(5)}",
            },
            ["value"] = new JsonArray($"{DateTimeOffset.Now.ToUnixTimeSeconds()}", $"{metrics}")
        };

        if (!string.IsNullOrEmpty(head))
        {
            var sha = $"{head.GetHashCode():X}".ToLower();
            tmp["metric"]["aks_name"] = $"{name}-{type}-{sha}";
            tmp["metric"]["github_head_branch"] = head;
            tmp["metric"]["aks_strategy"] = strategy;
        }
        
        return tmp;
    }

    public string GetEnvironments()
    {
        var set = new JsonArray();
        var result = new JsonObject()
        {
            ["status"] = "success",
            ["data"] = new JsonObject()
            {
                ["resultType"] = "vector",
                ["result"] = set
            }
        };
        
        var ns = new[] { "project", "planning", "demo" };
        var envs = new[] { "sit", "prd", "dev", "uat", "tst" };

        ns.ForEach(n => envs.ForEach(e =>
        {
            var item = new JsonObject()
            {
                ["metric"] = new JsonObject()
                {
                    ["aks_space"] = $"{n}",
                    ["namespace"] = $"{n + "-" + e}"
                },
                ["value"] = new JsonArray($"{DateTimeOffset.Now.ToUnixTimeSeconds()}", "30")
            };
            set.Add(item);
        }));
        
        return result.ToJsonString();
    }
    
    public static string RandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }

    public string GetAlerts()
    {
        var set = new JsonArray();
        var result = new JsonObject()
        {
            ["status"] = "success",
            ["data"] = new JsonObject()
            {
                ["resultType"] = "vector",
                ["result"] = set
            }
        };
        
        var alarms = new []{ 
            new { rep = "legacy-exporter", name = "p90-exporter", type ="worker", space = "demo", env = "prd", alarm ="P90 has exploded!"},
            new { rep = "graffiti", name = "graffiti", type ="api", space="planning", env = "prd", alarm= "Instance Down!"},
            new { rep = "graffiti", name = "graffiti", type ="ui", space="planning", env = "prd", alarm= "Instance Down!"},
            new { rep = "departure-board", name = "departure-board", type ="ui", space="demo", env="prd", alarm="IGP is hot garbage!"},
            new { rep = "dev-sec-ops", name = "dev-sec-ops", type ="worker", space="demo", env="prd", alarm="Passwords stored in clear text!" },
        };

        alarms.ForEach(x =>
        {
            var alert = GetAlert(x.space, x.env, x.rep, x.name, x.type, x.alarm);
            set.Add(alert);
        });
        
        return result.ToJsonString();
    }
    
    private JsonObject GetAlert(string ns, string env, string repo, string name, string type, string alert)
    {
        var deployed = DateTimeOffset.Now.AddDays(-Random.Shared.Next(1, 7)).ToUnixTimeSeconds();
        var result = new JsonObject()
        {
            ["metric"] = new JsonObject
            {
                ["__name__"] = "up",
                ["aks_deployed"] = $"{deployed}",
                ["aks_environment"] = env,
                ["aks_name"] = $"{name}-{type}",
                ["aks_space"] = ns,
                ["aks_systemid"] = "no",
                ["aks_type"] = type,
                ["aks_version"] = $"{DateTime.Now.AddDays(-Random.Shared.Next(1, 7)):yyyyMMdd}-{Random.Shared.Next(1,99)}-{RandomString(7)}",
                ["alertname"] = alert,
                ["app"] = $"{name}-{type}-{env}",
                ["github_branch"] = "main",
                ["github_org"] = "my-org",
                ["github_repository"] = repo,
                ["github_user"] = "HenrikDK",
                ["namespace"] = $"{ns}-{env}",
                ["pod"] = $"{name}-{type}-{RandomString(7)}-{RandomString(5)}"
            },
            ["value"] = new JsonArray($"{DateTimeOffset.Now.ToUnixTimeSeconds()}", "1")
        };
         
        return result;
    }
}