using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ErgoNodeSpyder.Portal.Health
{
    public class SpyderHealthCheck: IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            //We have to implement the checks that we need to do.
            return Task.FromResult(HealthCheckResult.Healthy("All systems normal"));
        }
    }
}
