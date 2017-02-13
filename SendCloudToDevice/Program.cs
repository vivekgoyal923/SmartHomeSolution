using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace SendCloudToDevice
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=SmartHomeIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=bj/atNOEcH/Omhp7PRv1DnLaL9mj0eMLmEyD70h7HWg=";

        static void Main(string[] args)
        {
            Console.WriteLine("Send Cloud-to-Device message\n");
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            Console.WriteLine("Press any key to send a C2D message.");
            while (true)
            {
                Console.ReadLine();
                SendCloudToDeviceMessageAsync().Wait();
            }
        }

        private async static Task SendCloudToDeviceMessageAsync()
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes("Cloud to device message."));
            await serviceClient.SendAsync("myFirstDevice", commandMessage);
        }
    }
}
