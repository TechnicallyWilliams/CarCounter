using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Newtonsoft.Json;



namespace Vision
{
    public class ServerResponse
    {
        [JsonProperty(PropertyName ="predictions")]
         public List<Prediction> Predictions { get; set; }

        public override string ToString()
        {

            return JsonConvert.SerializeObject(this);
        }
    }
    public class Prediction
    {
        [JsonProperty(PropertyName = "probability")]
        public float Probability { get; set; }
        [JsonProperty(PropertyName = "tagName")]
        public string TagName { get; set; }
    }
    public class HttpImageClassifier : IImageClassifier
    {
        public async Task<string> Classify(SoftwareBitmap image)
        {
            // First: Use an encoder to copy from SoftwareBitmap to an in-mem stream (FlushAsync)
            // Next: Use ReadAsync on the in-mem stream to get byte[] array

            //1) Convert SoftwareBitMap
            byte[] array = null;
            using (var ms = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms);
                encoder.SetSoftwareBitmap(image);
                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception ex) { throw new NotImplementedException(); }
                array = new byte[ms.Size];
                await ms.ReadAsync(array.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
            }

            string response = string.Empty;
            //Use mutlipartformdata to send frame to classifier api
            var containerURL = @"http://127.0.0.1:4000/image";
            response = await MultiForm_GetJsonData(containerURL, array);

            // parse response and get highest probability, if none, return null

            return response;

        }

        /// private async static Task<String> 

        /// <summary>
        /// GetHttpJason method gets the response from the customVision Docker API
        /// this assumes that the API is up and running change later if the 
        /// API has changed
        /// </summary>
        private async Task<string> MultiForm_GetJsonData(string url, byte[] currentFrame)
        {
            string response1 = String.Empty;

            using (var client = new HttpClient())
            {
                using (var content =
                new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(new MemoryStream(currentFrame)), "imageData", "PreviewFrame.jpg");
                    using (
                    var message =
                    await client.PostAsync(url, content))
                    {
                        response1 = await message.Content.ReadAsStringAsync();
                    }
                }
            }
            
            try
            {
                return GetPrediction(response1);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public string GetPrediction(string apires)
        {
            if (apires == null)
            {
                return null;
            }
            var decodedResponse = JsonConvert.DeserializeObject<ServerResponse>(apires);
            Debug.WriteLine(decodedResponse);
            if (decodedResponse == null)
            {
                return null;
            }

            if (decodedResponse.Predictions == null || decodedResponse.Predictions.Count == 0)
            {
                return null;
            }

            decodedResponse.Predictions.Sort((x, y) => x.Probability.CompareTo(y.Probability));
            var highestPrediction = decodedResponse.Predictions.Last();
            if (highestPrediction.Probability < 0.5)
            {
                return null;
            }
            if (highestPrediction.TagName == null)
            {
                return null;
            }
            if (highestPrediction.TagName.StartsWith("no-"))
            {
                return null;
            }
            return highestPrediction.TagName;
        }
    }
}
