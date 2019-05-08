using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    static class Upload
    {
        static Dictionary<Int64, MarketplaceOrder> MarketOrders = new Dictionary<long, MarketplaceOrder>();
        public static string APIURL = "https://localhost:5001/";
        static RestClient client = new RestClient(APIURL);

        public static void UploadPing(Ping p)
        {
            var request = new RestRequest("api/ping/", Method.POST);
            request.AddJsonBody(JsonConvert.SerializeObject(p));
            client.Execute(request);
        }
    }
}
