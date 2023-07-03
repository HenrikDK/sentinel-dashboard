using Sentinel.Dashboard.Ui.Model;
using Sentinel.Dashboard.Ui.Model.Repositories;

namespace Sentinel.Dashboard.Ui.Test.IntegrationTests;

public class PrometheusRepositoryTests
{
    private Container _container;
    private IConfiguration _configuration;
    
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
            x.AssemblyContainingType<IIssuesRepository>();
            x.WithDefaultConventions();
            x.LookForRegistries();
        });
        registry.AddSingleton(_configuration);
        registry.AddMemoryCache();
        _container = new Container(registry);
    }
    
    //[Test]
    public void Should_get_deployments()
    {
        var repository = _container.GetInstance<IPrometheusRepository>();

        var environments = repository.GetDeployments();

        environments.Should().NotBeNullOrEmpty();
    }
    
    //[Test]
    public void Should_get_environments()
    {
        var repository = _container.GetInstance<IPrometheusRepository>();

        var environments = repository.GetEnvironments();

        environments.Should().NotBeNullOrEmpty();
    }
    
    //[Test]
    public void Should_get_alerts()
    {
        var repository = _container.GetInstance<IPrometheusRepository>();

        var environments = repository.GetAlerts();

        environments.Should().NotBeNullOrEmpty();
    }
}