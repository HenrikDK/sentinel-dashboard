using Sentinel.Dashboard.Ui.Model;
using Sentinel.Dashboard.Ui.Model.Repositories;

namespace Sentinel.Dashboard.Ui.Test.IntegrationTests;

public class HumioRepositoryTests
{
    private Container _container;
    private IConfiguration _configuration;
    private Issue _issue;

    [SetUp]
    public void Setup()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var registry = new ServiceRegistry();
        registry.Scan(x =>
        {
            x.AssemblyContainingType<HumioRepository>();
            x.WithDefaultConventions();
            x.LookForRegistries();
        });
        registry.AddSingleton(_configuration);
        registry.AddMemoryCache();
        _container = new Container(registry);
        
        _issue = new Issue
        {
            MessageTemplate = "Exception updating data",
            SourceContext = "Some.Worker.UpdateData.UpdateData",
            Service = "some-worker"
        };
    }

    //[Test]
    public void Should_get_24hour_overview()
    {
        var repository = _container.GetInstance<IHumioRepository>();

        var issues = repository.GetOverview("app-tst");

        issues.Count.Should().BeGreaterThan(0);
        issues.Any(x => x.Service.Length > 0).Should().BeTrue ();
        issues.Any(x => x.MessageTemplate.Length > 0).Should().BeTrue();
        issues.Any(x => x.Events30Days > 0).Should().BeTrue();
        issues.Any(x => x.FirstSeen > DateTime.Now.AddDays(-14)).Should().BeTrue();
        issues.Any(x => x.LastSeen > DateTime.Now.AddDays(-30)).Should().BeTrue();
    }
    
    //[Test]
    public void Should_get_24hour_activity()
    {
        var repository = _container.GetInstance<IHumioRepository>();

        var activity = repository.GetActivity("app-tst", "24hours");

        activity.Count.Should().BeGreaterThan(0);
        activity.Any(x => x.Count > 0).Should().BeTrue();
        activity.Any(x => x.Bucket > DateTime.Now.AddDays(-2)).Should().BeTrue();
        activity.Any(x => x.Name.Length > 0).Should().BeTrue();
    }
    
    //[Test]
    public void Should_get_24_hour_issue_activity()
    {
        var repository = _container.GetInstance<IHumioRepository>();

        var activity = repository.GetIssueActivity("app-tst", "24hours", _issue);

        activity.Count.Should().BeGreaterThan(0);
        activity.Any(x => x.Count > 0).Should().BeTrue();
        activity.Any(x => x.Bucket > DateTime.Now.AddDays(-2)).Should().BeTrue();
        activity.Any(x => x.Name.Length > 0).Should().BeTrue();
    }
    
    //[Test]
    public void Should_get_14_day_error_activity()
    {
        var repository = _container.GetInstance<IHumioRepository>();

        var activity = repository.GetIssueActivity("app-tst", "14days", _issue);

        activity.Count.Should().BeGreaterThan(0);
        activity.Any(x => x.Count > 0).Should().BeTrue();
        activity.Any(x => x.Bucket > DateTime.Now.AddDays(-2)).Should().BeTrue();
        activity.Any(x => x.Name.Length > 0).Should().BeTrue();
    }
    
    //[Test]
    public void Should_get_30_day_error_activity()
    {
        var repository = _container.GetInstance<IHumioRepository>();
        var activity = repository.GetIssueActivity("app-tst", "30days", _issue);

        activity.Count.Should().BeGreaterThan(0);
        activity.Any(x => x.Count > 0).Should().BeTrue();
        activity.Any(x => x.Bucket > DateTime.Now.AddDays(-2)).Should().BeTrue();
        activity.Any(x => x.Name.Length > 0).Should().BeTrue();
    }
    
    //[Test]
    public void Should_get_environments_from_config()
    {
        var repository = _container.GetInstance<IHumioRepository>();

        var environments = repository.GetEnvironments();

        environments.Count.Should().BeGreaterThan(0);
    }
}