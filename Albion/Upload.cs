using Newtonsoft.Json;
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
        static List<Ping> pings = new List<Ping>();
        public static string APIURL = "https://localhost:5001/";
        static HttpClient client = new HttpClient();

        public static void UploadPings()
        {
            client.BaseAddress = new Uri(APIURL);
            pings.ForEach(async x =>
            {

                    
                var response = await client.PostAsync("api/ping/", new StringContent(JsonConvert.SerializeObject(x), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                    lock (pings)
                    {
                        pings.Remove(x);
                    }
            });
        }

        public static void AddPing(Ping p)
        {
            pings.Add(p);
        }
    }
}
