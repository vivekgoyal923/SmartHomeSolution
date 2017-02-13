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
using Microsoft.ProjectOxford.Face;
using System.IO;
using Microsoft.ProjectOxford.Face.Contract;

namespace Device.ViewModel
{
    class ViewVisitorViewModel:INotifyPropertyChanged
    {
        FaceServiceClient faceServiceClient;
        static DeviceClient deviceClient;
        static string iotHubUri = "SmartHomeIoTHub.azure-devices.net";
        static string deviceKey = "TdNTA6opJOsv50uPgjE6C1Mb5cLTyEKsiU63tFtLTVg=";
        CloudStorageAccount storageAccount;
        static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=imgstorageaccount;AccountKey=cT4774aJLYLi43WGXmLoUnBMz8cUKB8363+B/nikY9zvM7ojNh6KG2zBrX9bG5zydDK3Q0lYN0RRyC3lymBZJA==";
        StorageFile file, loading;

        public ICommand Allow { get; set; }
        public ICommand Deny { get; set; }
        public ICommand Refresh { get; set; }
        public ICommand AlwaysAllow { get; set; }
        public ICommand AlwaysDeny { get; set; }
        public ICommand AlwaysAsk { get; set; }
        public ICommand Save { get; set; }
        public ICommand Delete { get; set; }
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
        private string permissionStackVisibility;
        public string PermissionStackVisibility
        {
            get
            {
                return permissionStackVisibility;
            }
            set
            {
                permissionStackVisibility = value;
                OnPropertyChanged("PermissionStackVisibility");
            }
        }
        private string nameStackVisibility;
        public string NameStackVisibility
        {
            get
            {
                return nameStackVisibility;
            }
            set
            {
                nameStackVisibility = value;
                OnPropertyChanged("NameStackVisibility");
            }
        }
        private string personName;
        public string PersonName
        {
            get
            {
                return personName;
            }
            set
            {
                personName = value;
                OnPropertyChanged("PersonName");
            }
        }
        private string groupName;
        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                groupName = value;
                OnPropertyChanged("GroupName");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public ViewVisitorViewModel()
        {
            this.Allow = new RelayCommand(AllowButtonClick);
            this.Deny = new RelayCommand(DenyButtonClick);
            this.Refresh = new RelayCommand(ViewImage);
            this.AlwaysAllow = new RelayCommand(AlwaysAllowButtonClick);
            this.AlwaysDeny = new RelayCommand(AlwaysDenyButtonClick);
            this.AlwaysAsk = new RelayCommand(AlwaysAskButtonClick);
            this.Save = new RelayCommand(SaveButtonClick);
            this.Delete = new RelayCommand(DeleteButtonClick);
            PermissionStackVisibility = "Collapsed";
            NameStackVisibility = "Collapsed";
            faceServiceClient = new FaceServiceClient("fbeb228a7f1943b69c3e6e2861f22ff0");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("myFirstDevice", deviceKey), Microsoft.Azure.Devices.Client.TransportType.Http1);
            storageAccount = CloudStorageAccount.Parse(storageConnectionString); //Retrieve storage account from connection string.
            ViewImage();
        }

        private async void DeleteButtonClick()
        {
            Person[] persons = await faceServiceClient.GetPersonsAsync(GroupName);
            Person person;
            Guid personId;
            try
            {
                person = persons.Where(item => item.Name == personName).First();
                personId = person.PersonId;
                await faceServiceClient.DeletePersonAsync(GroupName, personId);
                TestText += "\nPerson deleted successfuly";
            }
            catch
            {
                TestText += "\nPerson doesnt exist in the list";
            }
        }

        private async void SaveButtonClick()
        {
            if (PersonName != "")
            {

                string personGroupId = "group1";
                Stream image = await file.OpenStreamForReadAsync();

                Person[] persons = await faceServiceClient.GetPersonsAsync(personGroupId);
                Person person;
                Guid personId;
                try{
                    person = persons.Where(item => item.Name == personName).First();
                    personId = person.PersonId;
                }
                catch
                {
                    CreatePersonResult friend1 = await faceServiceClient.CreatePersonAsync(
                            personGroupId,
                            personName
                        );
                    personId = friend1.PersonId;
                }
                await faceServiceClient.AddPersonFaceAsync(personGroupId, personId, image);

                await faceServiceClient.TrainPersonGroupAsync(personGroupId);
                NameStackVisibility = "Collapsed";
                TestText += "\nThe Person will automatically be allowed next time";

            }
            
        }

        private void AlwaysAskButtonClick()
        {
            PermissionStackVisibility = "Collapsed";
            TestText += "\n Setting has been saved";
        }

        private void AlwaysDenyButtonClick()
        {
            PermissionStackVisibility = "Collapsed";

            TestText += "\n Person will never allowed entry";
        }

        private void AlwaysAllowButtonClick()
        {
            PermissionStackVisibility = "Collapsed";
            NameStackVisibility = "Visible";
        }

        private async void DenyButtonClick()
        {
            var telemetryDataPoint = new
            {
                deviceId = "secondDevice",
                status = "deny",
                name = ""
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
            PermissionStackVisibility = "Collapsed";
            TestText += "\n Person is Denied the entry";
        }

        private async void AllowButtonClick()
        {
            var telemetryDataPoint = new
            {
                deviceId = "secondDevice",
                status = "allow",
                name = ""
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);

            PermissionStackVisibility = "Visible";
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
            TestText = "";
             // Save blob contents to a file.
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
