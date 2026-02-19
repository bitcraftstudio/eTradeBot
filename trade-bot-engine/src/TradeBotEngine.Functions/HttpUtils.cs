using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TradeBotEngine.Functions
{
    public class HttpUtils
    {
        public static readonly JsonSerializerOptions JsonOutOpts = new() 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }  // Serialize enums as strings
        };
        
        public static readonly JsonSerializerOptions JsonInOpts = new() 
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }  // Deserialize enums from strings
        };
        
        public static async Task<HttpResponseData> JsonOk(HttpRequestData req, object data)
        {
            var res = req.CreateResponse(HttpStatusCode.OK);
            res.Headers.Add("Content-Type", "application/json");
            await res.WriteStringAsync(JsonSerializer.Serialize(data, JsonOutOpts));
            return res;
        }

        public static async Task<HttpResponseData> JsonCustom(HttpRequestData req, HttpStatusCode statusCode, object data)
        {
            var res = req.CreateResponse(statusCode);
            res.Headers.Add("Content-Type", "application/json");
            await res.WriteStringAsync(JsonSerializer.Serialize(data, JsonOutOpts));
            return res;
        }

        public static async Task<HttpResponseData> JsonOther(HttpRequestData req, HttpStatusCode status, string message)
        {
            var res = req.CreateResponse(status);
            res.Headers.Add("Content-Type", "application/json");
            await res.WriteStringAsync(JsonSerializer.Serialize(new { success = false, error = message }, JsonOutOpts));
            return res;
        }
    }
}
