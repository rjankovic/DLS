//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace CD.DLS.Clients.Web.Models
//{
//    public enum RouteController { };
//    public enum RouteAction { };

//    public class RouteArguments
//    {


//        public string ArgumentsString { get { return Serialize(); } }
//        public string Controller { get; set; }
//        public string Action { get; set; }

//        public string Serialize()
//        {
//            JsonSerializerSettings settings = new JsonSerializerSettings
//            {
//                TypeNameHandling = TypeNameHandling.All
//            };
//            return JsonConvert.SerializeObject(this, settings);
//        }

//        public static RouteArguments Deserialize(string serialized)
//        {
//            JsonSerializerSettings settings = new JsonSerializerSettings
//            {
//                TypeNameHandling = TypeNameHandling.All
//            };
//            return JsonConvert.DeserializeObject<RouteArguments>(serialized, settings);
//        }
//    }
//}