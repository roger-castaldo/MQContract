using Microsoft.Extensions.Diagnostics.HealthChecks;
using MQContract.Interfaces.Service;
using MQContract.Messages;
namespace MQContract.Connections
{
    internal class ConnectionHealthCheck : IHealthCheck
    {
        private const string HealthyDescription = "MQContract service connection available";
        private const string UnHealthyDescription = "MQContract service connection unavailable";

        private sealed record ServicePingResult(string ServiceName,PingResult? Result = null,Exception? Error = null);

        private readonly IPingableMessageServiceConnection? connection;
        private readonly ServiceConnectionList? serviceConnectionList;

        public ConnectionHealthCheck(IMessageServiceConnection? connection=null, ServiceConnectionList? serviceConnectionList = null)
        {
            if (connection==null && serviceConnectionList==null)
                throw new ArgumentNullException(string.Empty,"Either connection or serviceConnectionList needs to be provided");
            if (serviceConnectionList!=null && !serviceConnectionList.FullList.Any(conn => conn.MessageServiceConnection is IPingableMessageServiceConnection))
                throw new ArgumentOutOfRangeException(nameof(serviceConnectionList), "No Pingable service connections provided, cannot provide health checks");
            else if (connection!=null && connection is not IPingableMessageServiceConnection)
                throw new ArgumentOutOfRangeException(nameof(connection), "Service connection is not Pingable");
            this.connection=(IPingableMessageServiceConnection?)connection;
            this.serviceConnectionList=serviceConnectionList;
        }

        private static IReadOnlyDictionary<string, object> MapPing(PingResult result)
            => new Dictionary<string, object>()
            {
                { "hostname",result.Host},
                { "version",result.Version},
                { "responseTime",result.ResponseTime }
            };

        private static IReadOnlyDictionary<string,object> MapServicePingResult(ServicePingResult servicePingResult)
            => (servicePingResult.Error==null ? MapPing(servicePingResult.Result!) : new Dictionary<string, object>() { { "error", servicePingResult.Error.Message } });

        async Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            if (connection!=null)
            {
                try
                {
                    var result = await connection.PingAsync();
                    return HealthCheckResult.Healthy(HealthyDescription, MapPing(result));
                }
                catch (Exception e)
                {
                    return HealthCheckResult.Unhealthy(UnHealthyDescription, e);
                }
            }
            else
            {
                var results = await Task.WhenAll(serviceConnectionList!.FullList
                    .Where(conn=>conn.MessageServiceConnection is IPingableMessageServiceConnection)
                    .Select(async (conn) =>
                    {
                        try
                        {
                            var result = await ((IPingableMessageServiceConnection)conn.MessageServiceConnection).PingAsync();
                            return new ServicePingResult(conn.ServiceConnectionName, Result: result);
                        }
                        catch(Exception e)
                        {
                            return new ServicePingResult(conn.ServiceConnectionName, Error: e);
                        }
                    })
                );
                var data = new Dictionary<string, object>(results.Select(r =>
                    new KeyValuePair<string, object>(r.ServiceName, MapServicePingResult(r))
                ));
                if (!Array.Exists(results, r => r.Error!=null))
                    return HealthCheckResult.Healthy(HealthyDescription, data: data);
                return HealthCheckResult.Unhealthy(UnHealthyDescription, data: data);
            }
        }
    }
}
