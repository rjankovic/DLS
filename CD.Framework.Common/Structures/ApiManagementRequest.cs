using CD.DLS.Common.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Common.Structures
{
    public abstract class ApiManagementRequest
    {
        public string Serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static ApiManagementRequest Deserialize(string serialized)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject<ApiManagementRequest>(serialized, settings);
        }
    }

    public class ClearCacheApiManagementRequest : ApiManagementRequest
    {
        public CoreTypeEnum? CoreType { get; set; }
    }

    public class ConfigurationApiManagementRequest : ApiManagementRequest
    {
    }

    public class ProjectListApiManagementRequest : ApiManagementRequest
    {
    }
}