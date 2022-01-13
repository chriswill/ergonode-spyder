using System.Threading;
using System.Threading.Tasks;
using ErgoNodeSharp.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ErgoNodeSpyder.Portal.Health
{
    public class SpyderHealthCheck: IHealthCheck
    {
        private readonly INodeReportingRepository reportingRepository;

        public SpyderHealthCheck(INodeReportingRepository reportingRepository)
        {
            this.reportingRepository = reportingRepository;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            int nodeCount = await reportingRepository.GetNodeInfoCount();
            return nodeCount == 0 ? 
                HealthCheckResult.Unhealthy("Node count is zero") : 
                HealthCheckResult.Healthy($"{nodeCount} nodes reported");
        }
    }
}
