using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Azure.NotificationHubs;

namespace SimulatedDevice
{
    class Program
    {
        static DeviceClient deviceClient;
        static DeviceClient deviceClient2;
        static string iotHubUri = "SmartHomeIoTHub.azure-devices.net";
        static string deviceKey = "TdNTA6opJOsv50uPgjE6C1Mb5cLTyEKsiU63tFtLTVg=";
        static string deviceKey2 = "P+k5ij9loAaQTNDprAh3CoTVmV/9L0W3ZkamYTDk16k=";

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("myFirstDevice", deviceKey), TransportType.Http1);
            deviceClient2 = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("mySecondDevice", deviceKey2), TransportType.Http1);
            SendDeviceToCloudMessagesAsync();
            ReceiveC2dAsync();
            //SendToBlobAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double avgWindSpeed = 10; // m/s
            Random rand = new Random();

            while (true)
            {
                double currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;

                var telemetryDataPoint = new
                {
                    deviceId = "myFirstDevice",
                    windSpeed = currentWindSpeed
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                Task.Delay(1000).Wait();
            }
        }

        private static async void ReceiveC2dAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                    Console.ResetColor();
                    await deviceClient.CompleteAsync(receivedMessage);
                }
                Message receivedMessage2 = await deviceClient2.ReceiveAsync();
                if (receivedMessage2 != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage2.GetBytes()));
                    Console.ResetColor();
                    await deviceClient2.CompleteAsync(receivedMessage2);
                }
                //SendNotificationAsync("Visitor.jpg");
            }
        }
        private static async void SendNotificationAsync(string BlobName)
        {
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString("Endpoint=sb://smarthomenamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=+nY8fngDiGjuWma5CTi3KxEUV/jwyE9GAxv7Nrf4FW0=", "SmartHomeNotificationHub");
            var toast = @"<toast><visual><binding template=""ToastText01""><text id=""1"">New Visitor at your House!! Please Check " + BlobName + "</text></binding></visual></toast>";
            await hub.SendWindowsNativeNotificationAsync(toast);
        }
        private static async void SendToBlobAsync()
        {

            string fileName = "FirstTestImage.jpg";
            Console.WriteLine("Uploading file: {0}", fileName);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            using (var sourceData = new FileStream(@"TestImage.jpg", FileMode.Open))
            {
                await deviceClient.UploadToBlobAsync(fileName, sourceData);
            }
            watch.Stop();
            Console.WriteLine("Time to upload file: {0}ms\n", watch.ElapsedMilliseconds);
        }


    }
}
