using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace Device.ViewModel
{
    class ViewVisitorViewModel:INotifyPropertyChanged
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "SmartHomeIoTHub.azure-devices.net";
        static string deviceKey = "TdNTA6opJOsv50uPgjE6C1Mb5cLTyEKsiU63tFtLTVg=";
        CloudStorageAccount storageAccount;
        static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=imgstorageaccount;AccountKey=cT4774aJLYLi43WGXmLoUnBMz8cUKB8363+B/nikY9zvM7ojNh6KG2zBrX9bG5zydDK3Q0lYN0RRyC3lymBZJA==";

        public ICommand Allow { get; set; }
        public ICommand Deny { get; set; }
        public ICommand Refresh { get; set; }
        private Uri image;
        public Uri Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                OnPropertyChanged("Image");
            }
        }
        private string testText;
        public string TestText
        {
            get
            {
                return testText;
            }
            set
            {
                testText = value;
                OnPropertyChanged("TestText");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewVisitorViewModel()
        {
            this.Allow = new RelayCommand(AllowButtonClick);
            this.Deny = new RelayCommand(DenyButtonClick);
            this.Refresh = new RelayCommand(ViewImage);
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("myFirstDevice", deviceKey), Microsoft.Azure.Devices.Client.TransportType.Http1);
            storageAccount = CloudStorageAccount.Parse(storageConnectionString); //Retrieve storage account from connection string.
            ViewImage();
        }

        private async void DenyButtonClick()
        {
            var telemetryDataPoint = new
            {
                deviceId = "secondDevice",
                status = "deny"
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
        }

        private async void AllowButtonClick()
        {
            var telemetryDataPoint = new
            {
                deviceId = "secondDevice",
                status = "allow"
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private async void ViewImage()
        {
            StorageFile file, loading; // Save blob contents to a file.
            Windows.Storage.StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
            loading = await temporaryFolder.GetFileAsync("loading.gif");
            Image = new BitmapImage(new Uri(loading.Path)).UriSource;
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient(); // Create the blob client.
            CloudBlobContainer container = blobClient.GetContainerReference("imgcontainer"); // Retrieve reference to a previously created container.
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("Visitor.jpg"); // Retrieve reference to a blob named "photo1.jpg".
            file = await temporaryFolder.CreateFileAsync("WhoIsIt.jpg",
                CreationCollisionOption.ReplaceExisting);
            await blockBlob.DownloadToFileAsync(file);
            Image = new BitmapImage(new Uri(file.Path)).UriSource;
            TestText = "Image Downloaded from Azure Storage Account";
        }
    }
}
