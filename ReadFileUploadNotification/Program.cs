using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.NotificationHubs;

namespace ReadFileUploadNotification
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=SmartHomeIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=bj/atNOEcH/Omhp7PRv1DnLaL9mj0eMLmEyD70h7HWg=";
        static void Main(string[] args)
        {
            Console.WriteLine("Receive file upload notifications\n");
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            ReceiveFileUploadNotificationAsync().Wait();
            Console.ReadLine();

        }

        private async static Task ReceiveFileUploadNotificationAsync()
        {
            var notificationReceiver = serviceClient.GetFileNotificationReceiver();

            Console.WriteLine("\nReceiving file upload notification from service");
            while (true)
            {
                var fileUploadNotification = await notificationReceiver.ReceiveAsync();
                if (fileUploadNotification == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                SendNotificationAsync(fileUploadNotification.BlobName);
                Console.WriteLine("Received file upload noticiation: {0}", string.Join(", ", fileUploadNotification.BlobName));
                Console.ResetColor();
                await notificationReceiver.CompleteAsync(fileUploadNotification);
            }
        }
        private static async void SendNotificationAsync(string BlobName)
        {
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString("Endpoint=sb://smarthomenamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=+nY8fngDiGjuWma5CTi3KxEUV/jwyE9GAxv7Nrf4FW0=", "SmartHomeNotificationHub");
            var toast = @"<toast><visual><binding template=""ToastText01""><text id=""1"">New Visitor at your House!! Please Check " + BlobName + "</text></binding></visual></toast>";
            await hub.SendWindowsNativeNotificationAsync(toast);
        }
    }
}
