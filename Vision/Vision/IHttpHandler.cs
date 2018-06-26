using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Vision
{
    public interface IHttpHandler
    {
        Task<HttpResponseMessage> PostAsync(Uri url, StringContent content);
    }
}
