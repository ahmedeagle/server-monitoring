using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Application.Interfaces;

public interface IMetricsCollector
{
    Task<Metric> CollectMetricsAsync(Server server, CancellationToken cancellationToken = default);
}
