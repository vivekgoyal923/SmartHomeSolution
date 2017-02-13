using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.IO;
using System.ComponentModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Windows.Media.Capture;
using Windows.Foundation;
using Windows.Media.MediaProperties;
using Microsoft.ProjectOxford.Face;

namespace CaptureImageAndUpload.ViewModel
{
    class UploadImageViewModel:INotifyPropertyChanged
    {
        FaceServiceClient faceServiceClient;
        static DeviceClient deviceClient;
        static string iotHubUri = "SmartHomeIoTHub.azure-devices.net";
        static string deviceKey = "TdNTA6opJOsv50uPgjE6C1Mb5cLTyEKsiU63tFtLTVg=";
        CloudStorageAccount storageAccount;
        StorageFile photo;
        static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=imgstorageaccount;AccountKey=cT4774aJLYLi43WGXmLoUnBMz8cUKB8363+B/nikY9zvM7ojNh6KG2zBrX9bG5zydDK3Q0lYN0RRyC3lymBZJA==";
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand UploadImage { get; set; }
        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public UploadImageViewModel()
        {
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("myFirstDevice", deviceKey), Microsoft.Azure.Devices.Client.TransportType.Http1);
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            faceServiceClient = new FaceServiceClient("fbeb228a7f1943b69c3e6e2861f22ff0");
            ReceiveC2dAsync();
            this.UploadImage = new RelayCommand(UploadImageButtonClick);
            
        }
        private async void UploadImageButtonClick()
        {
            //CameraCaptureUI captureUI = new CameraCaptureUI();
            //captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            //captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);
            //photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            MediaCapture mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();

            photo = await KnownFolders.PicturesLibrary.CreateFileAsync(
                "capture_file.jpg", CreationCollisionOption.ReplaceExisting);

            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
            await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photo);

            if (photo == null)
            {
                // User cancelled photo capture
                return;
            }

            Status += "Image is Uploading...\n";
            SendToBlobAsync();

        }

        private async void SendToBlobAsync()
        {

            //var watch = System.Diagnostics.Stopwatch.StartNew();
            bool matchFound = false;
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient(); // Create the blob client.
            CloudBlobContainer container = blobClient.GetContainerReference("imgcontainer"); // Retrieve reference to a previously created container.
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("Visitor.jpg"); // Retrieve reference to a blob named "photo1.jpg".

            await blockBlob.UploadFromFileAsync(photo);
            //watch.Stop();
            Status += "Pic Uploaded\n";
            string personName = "New Visitor";
            Stream image = await photo.OpenStreamForReadAsync();
            try
            {
                var faces = await faceServiceClient.DetectAsync(image);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                var results = await faceServiceClient.IdentifyAsync("group1", faceIds);

                foreach (var identifyResult in results)
                {
                    if (identifyResult.Candidates.Length == 0)
                    {
                        matchFound = false;
                    }
                    else
                    {
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceServiceClient.GetPersonAsync("group1", candidateId);
                        personName = person.Name;
                        matchFound = true;
                        break;
                    }
                }
                if (matchFound)
                {
                    var telemetryDataPoint = new
                    {
                        deviceId = "myFirstDevice",
                        status = "Match Found",
                        name = personName
                    };
                    var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                    var message = new Message(Encoding.ASCII.GetBytes(messageString));

                    await deviceClient.SendEventAsync(message);
                }
                else
                {
                    var telemetryDataPoint = new
                    {
                        deviceId = "myFirstDevice",
                        status = "Pic Uploaded",
                        name = personName
                    };
                    var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                    var message = new Message(Encoding.ASCII.GetBytes(messageString));

                    await deviceClient.SendEventAsync(message);
                }
            }
            catch
            {
                var telemetryDataPoint = new
                {
                    deviceId = "myFirstDevice",
                    status = "Pic Uploaded",
                    name = personName
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
                Status += "\nThe Image is Not Clear\n";
            }
        }
        private async void ReceiveC2dAsync()
        {
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;
                Status += "Received message: " + Encoding.ASCII.GetString(receivedMessage.GetBytes()) + "\n";

                await deviceClient.CompleteAsync(receivedMessage);
            }
        }
    }
}
