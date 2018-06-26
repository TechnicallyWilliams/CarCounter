using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Vision
{
    public class HttpHandler : IHttpHandler
    {
        private HttpClient _client = new HttpClient();

        public async Task<HttpResponseMessage> PostAsync(Uri url, StringContent content)
        {
            return await _client.PostAsync(url, content);
        }
    }
}
