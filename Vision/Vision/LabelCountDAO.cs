using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;

namespace Vision
{
    public class LabelCountDAO : ILabelCountDAO
    {
        public Lazy<IHttpHandler> httpHandler { get; set; }

        public LabelCountDAO()
        {
            httpHandler = new Lazy<IHttpHandler>(() => new HttpHandler());
        }

        public async Task Save(LabelCount labelCount)
        {
            Uri baseUri = new Uri(@"http://localhost:5000/leapvision/api/v1.0/labeled_objects");
            var content = new StringContent(JsonConvert.SerializeObject(labelCount), Encoding.UTF8, "application/json");

            try
            {
                var client = httpHandler.Value;
                using (var message = await client.PostAsync(baseUri, content))
                {
                    if (message != null)
                    {
                        message.EnsureSuccessStatusCode();
                        Debug.Write(message.StatusCode);
                        var response = await message.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Write(e.InnerException.InnerException);
            }

        }
    }
}
