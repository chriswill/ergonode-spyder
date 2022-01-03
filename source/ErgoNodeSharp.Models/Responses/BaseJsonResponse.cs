using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ErgoNodeSharp.Models.Responses
{
    public abstract class BaseJsonResponse
    {
        [JsonProperty("jsonapi", Order = 1)]
        public JsonApi JsonApi { get; set; }

        [JsonProperty("meta", Order = 2)]
        public Meta Meta { get; set; }

        [JsonProperty("links", Order = 3)]
        public Links Links { get; set; }
        
        protected BaseJsonResponse()
        {
            JsonApi = new JsonApi();
            Meta = new Meta();
            Links = new Links { Last = null, Next = null, Prev = null};
        }
    }

    public class JsonApiResponse<T> : BaseJsonResponse where T : class
    {
        [JsonProperty("data", Order = 4)]
        public IEnumerable<T> Data { get; set; }

        public JsonApiResponse()
        {
            Meta.Pages = 1;
        }

    }

    public class Links
    {
        public string Self { get; set; }

        public string First { get; set; }

        public string Last { get; set; }

        public string Prev { get; set; }

        public string Next { get; set; }
    }

    public class Meta
    {
        public string Copyright => $"Copyright {DateTime.UtcNow.Year}, CloudScope LLC";
        public List<string> Authors { get; set; }
        public int TotalRecords { get; set; }
        public int Pages { get; set; }

        public Meta()
        {
            Authors = new List<string> { "Ergo Node Spyder Authors" };
        }
    }

    public class JsonApi
    {
        public string Version => "1.0";
    }
}
