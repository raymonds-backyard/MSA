using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using System.Net.Http.Headers;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MSA_app
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            
        }

        private void loadNewPage(object sender, EventArgs e)
        {

            Navigation.PushAsync(new EasyTables());
        }


        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Large,
                Directory = "Sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });

            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });

            await MakeRequest(file);
            

        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }


        async Task MakeRequest(MediaFile file)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid key.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "b82c923964be4fff97eee9c160ce6b61");

            string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
            HttpResponseMessage response;
            

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                LoadingSpinner.IsVisible = true;
                LoadingSpinner.IsRunning = true;
                response = await client.PostAsync(uri, content);
                LoadingSpinner.IsRunning = false;
                LoadingSpinner.IsVisible = false;
                try
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    List<EmotionModel> responseModel = JsonConvert.DeserializeObject<List<EmotionModel>>(responseContent);
                    if (responseModel.Count == 0){
                        dedLabel.Text = "Depress-o-meter : Ghosts don't have emotions";

                    }
                    else
                    {
                        double dedValue = Math.Round((responseModel[0].scores.neutral + responseModel[0].scores.sadness) * 100, 0);
                        dedLabel.Text = $"Depress-o-meter : {dedValue}% dead";
                        UpdateAzureTable(dedValue);
                    }

                }
                catch (Exception e)
                {
                    dedLabel.Text = "Depress-o-meter : So dead that the app died.";
                }

                file.Dispose();
            }

        }

        private async void UpdateAzureTable(double dedValue)
        {
            deadTable newEntry = new deadTable
            {
                DateUtc = DateTime.Now,
                SadLevel = dedValue.ToString()
            };
            await AzureManager.AzureManagerInstance.PostInfo(newEntry);
        }
    }
}