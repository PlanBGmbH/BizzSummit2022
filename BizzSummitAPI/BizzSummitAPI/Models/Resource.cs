using Newtonsoft.Json;
using System;

namespace BizzSummitAPI
{
    public class Resource
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "fullName")]
        public string FullName { get; set; }

       [JsonProperty(PropertyName = "mail")]
        public string Mail { get; set; }
    }
}
