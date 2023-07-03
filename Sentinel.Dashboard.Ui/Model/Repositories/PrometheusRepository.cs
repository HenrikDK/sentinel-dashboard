namespace Sentinel.Dashboard.Ui.Model.Repositories;

public interface IPrometheusRepository
{
    string GetDeployments();
    string GetEnvironments();
    string GetAlerts();
}

public class PrometheusRepository : IPrometheusRepository
{

}