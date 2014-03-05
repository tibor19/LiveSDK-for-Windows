using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OneDriveIntegration.JsonData
{
    public class ResourceResult
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "from")]
        public OwnerResult Owner { get; set; }
        [JsonProperty(PropertyName = "link")]
        public OwnerResult Link { get; set; }
    }
}
