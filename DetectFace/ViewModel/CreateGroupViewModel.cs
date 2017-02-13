using GalaSoft.MvvmLight.Command;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace DetectFace.ViewModel
{
    class CreateGroupViewModel
    {
        FaceServiceClient faceServiceClient;
        StorageFile photo;
        public ICommand CreateGroupButton { get; set; }
        public CreateGroupViewModel()
        {
            this.CreateGroupButton = new RelayCommand(CreateGroupButtonMethod);
        }

        private async void CreateGroupButtonMethod()
        {
            faceServiceClient = new FaceServiceClient("fbeb228a7f1943b69c3e6e2861f22ff0");

            //var client = new HttpClient();
            //var queryString = WebUtility.UrlEncode(string.Empty);
            string personGroupId = "group1";
            Guid personId = new Guid("a3f0bd5c-b917-41f7-b50f-ae6fb2e3c6a2");
            //// Request header
            //client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "fbeb228a7f1943b69c3e6e2861f22ff0");
            //var uri = "https://westus.api.cognitive.microsoft.com/face/v1.0/persongroups/" + personGroupId + "/persons/"+ personId +"/persistedFaces" ;

            //HttpResponseMessage response;

            MediaCapture mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();

            photo = await KnownFolders.PicturesLibrary.CreateFileAsync(
                "capture1.jpg", CreationCollisionOption.ReplaceExisting);

            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
            await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photo);

            //Request body
            Stream image = await photo.OpenStreamForReadAsync();
            await faceServiceClient.AddPersonFaceAsync(personGroupId, personId, image);

            await faceServiceClient.TrainPersonGroupAsync(personGroupId);


            //var faces = await faceServiceClient.DetectAsync(image);
            //var faceIds = faces.Select(face => face.FaceId).ToArray();

            //var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
            //foreach (var identifyResult in results)
            //{
            //    if (identifyResult.Candidates.Length == 0)
            //    {
                    
            //    }
            //    else
            //    {
            //        var candidateId = identifyResult.Candidates[0].PersonId;
            //        var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                    
            //    }
            //}
            //string body = JsonConvert.SerializeObject(new
            //{
            //    url =  photo.Path,
            //});
            //byte[] byteData = Encoding.UTF8.GetBytes(body);

            //using (var content = new ByteArrayContent(byteData))
            //{
            //    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            //    response = await client.PostAsync(uri, content);
            //}
        }
    }
}
