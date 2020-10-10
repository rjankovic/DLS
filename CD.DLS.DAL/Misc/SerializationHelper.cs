using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Misc
{
    public static class SerializationHelper
    {
        public static void PopulateExtendedProperties(object obj, string extendedProperties)
        {
            JsonConvert.PopulateObject(extendedProperties, obj);
        }

        public static string ReflectExtendedProperties(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static JObject ParseObject(string serialized)
        {
            return JObject.Parse(serialized);
        }

        //public static int? ParseLength(string extendedProperties)
        //{
        //    var o = ParseObject(extendedProperties);
        //    if (o.ContainsKey(EXT_PROP_LENGTH))
        //    {
        //        return o[EXT_PROP_LENGTH].Value<int>();
        //    }
        //    return null;
        //}

        //public static int? ParseOffsetFrom(string extendedProperties)
        //{
        //    var o = ParseObject(extendedProperties);
        //    if (o.ContainsKey(EXT_PROP_OFFSET_FROM))
        //    {
        //        return o[EXT_PROP_OFFSET_FROM].Value<int>();
        //    }
        //    return null;
        //}

        //public const string EXT_PROP_LENGTH = "Length";
        //public const string EXT_PROP_OFFSET_FROM = "OffsetFrom";
    }
}
