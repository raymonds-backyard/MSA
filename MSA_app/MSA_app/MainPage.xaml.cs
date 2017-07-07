﻿using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using System.Net.Http.Headers;

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
                response = await client.PostAsync(uri, content);

                try
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    List<EmotionModel> responseModel = JsonConvert.DeserializeObject<List<EmotionModel>>(responseContent);
                    double dedValue = Math.Round(responseModel[0].scores.neutral * 100, 0);
                    dedLabel.Text = $"You are {dedValue}% deadpan";
                }
                catch (Exception e)
                {
                    dedLabel.Text = System.Convert.ToString(e);
                }

                file.Dispose();
            }

            
            
        }
    }
}