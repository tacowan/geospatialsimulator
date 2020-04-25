using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.EventHubs;
using System.Text;

namespace FunctionApp
{
    public static class Functions
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "map")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("messages")]
        public static Task SendMessage(
            [EventHubTrigger("trucklocation", Connection = "EventHubConnectionAppSetting")] EventData myEvent,
            DateTime enqueuedTimeUtc,
            Int64 sequenceNumber,
            string offset,
            ILogger log,
            [SignalR(HubName = "map")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var msg = Encoding.UTF8.GetString(myEvent.Body.Array);
            log.LogInformation($"SequenceNumber={myEvent.SystemProperties.SequenceNumber}");
            var source = myEvent.SystemProperties["iothub-message-source"];
            if ("Telemetry".Equals(source))
            {
                var deviceId = myEvent.SystemProperties["iothub-connection-device-id"];
                return signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "broadcastMessage",
                        Arguments = new[] {  deviceId, msg }
                    });
            } else {
                return Task.CompletedTask; 
            }
        }
    }
}
