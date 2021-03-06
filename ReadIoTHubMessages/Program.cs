﻿using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReadIoTHubMessages
{
    class Program
    {
        static ServiceClient serviceClient;
        static string hubconnectionString = "HostName=SmartHomeIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=bj/atNOEcH/Omhp7PRv1DnLaL9mj0eMLmEyD70h7HWg=";

        static string connectionString = "HostName=SmartHomeIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=bj/atNOEcH/Omhp7PRv1DnLaL9mj0eMLmEyD70h7HWg=";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;
        static void Main(string[] args)
        {
            Console.WriteLine("Receive messages. Ctrl-C to exit.\n");
            serviceClient = ServiceClient.CreateFromConnectionString(hubconnectionString);
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

                string data = Encoding.UTF8.GetString(eventData.GetBytes());

                Console.WriteLine("Data: '{0}'", data);

                var message = JObject.Parse(data);
                string deviceId = message["device"].ToString();


                var commandMessage = new Message(Encoding.ASCII.GetBytes(data));

                if (deviceId == "01:01:01:01:01:01")
                    await serviceClient.SendAsync("myFirstDevice", commandMessage);
                else if (deviceId == "02:02:02:02:02:02")
                    await serviceClient.SendAsync("mySecondDevice", commandMessage);
                else if (deviceId == "03:03:03:03:03:03")
                    await serviceClient.SendAsync("myFourthDevice", commandMessage);
                else if (deviceId == "04:04:04:04:04:04")
                    await serviceClient.SendAsync("myFifthDevice", commandMessage);
                else
                    await serviceClient.SendAsync("myThirdDevice", commandMessage);

            }
        }
    }
}
