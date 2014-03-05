using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OneDriveIntegration.JsonData
{
    public class ResourceListResult
    {
        [JsonProperty(PropertyName = "data")]
        public IList<ResourceResult> ResourceResults { get; set; }
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
