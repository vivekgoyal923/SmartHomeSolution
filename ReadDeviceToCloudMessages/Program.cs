using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Threading;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.Devices;
using Newtonsoft.Json.Linq;

namespace ReadDeviceToCloudMessages
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=SmartHomeIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=bj/atNOEcH/Omhp7PRv1DnLaL9mj0eMLmEyD70h7HWg=";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        static void Main(string[] args)
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            Console.WriteLine("Receive messages. Ctrl-C to exit.\n");
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;
            CancellationTokenSource cts = new CancellationTokenSource();

            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();
            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
            }
            Task.WaitAll(tasks.ToArray());

        }

        private static async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                EventData eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;

                var data = Encoding.UTF8.GetString(eventData.GetBytes());
                Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);

                var message = JObject.Parse(data);
                string deviceId = message["deviceId"].ToString();
                if (deviceId == "myFirstDevice")
                {
                    SendNotificationAsync("Visitor.jpg");
                }
                else if(deviceId == "secondDevice")
                {
                    string status = message["status"].ToString();
                    if (status == "allow")
                    {
                        SendCloudToDeviceMessageAsync("Allow").Wait();
                    }
                    else if (status == "deny")
                    {
                        SendCloudToDeviceMessageAsync("Deny").Wait();
                    }
                }

            }
        }
        private static async void SendNotificationAsync(string BlobName)
        {
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString("Endpoint=sb://smarthomenamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=+nY8fngDiGjuWma5CTi3KxEUV/jwyE9GAxv7Nrf4FW0=", "SmartHomeNotificationHub");
            var toast = @"<toast><visual><binding template=""ToastText01""><text id=""1"">New Visitor at your House!! Please Check " + BlobName + "</text></binding></visual></toast>";
            await hub.SendWindowsNativeNotificationAsync(toast);
        }
        private async static Task SendCloudToDeviceMessageAsync(string message)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(message));
            await serviceClient.SendAsync("myFirstDevice", commandMessage);
        }

    }
}
