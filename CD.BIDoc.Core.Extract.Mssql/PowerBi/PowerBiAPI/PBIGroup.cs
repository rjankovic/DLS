using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using System.Net.Http;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    public class PBIGroup : Group, IPBIObject
    {
        #region Private Properties for Serialization
        [JsonProperty(PropertyName = "@odata.context", NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        private string ODataContext;

        private string _apiURL = null;
        #endregion

            
        public List<PBIReport> Reports
        {
            get
            {
                PBIObjectList<PBIReport> objList = JsonConvert.DeserializeObject<PBIObjectList<PBIReport>>(ParentPowerBIAPI.SendGETRequest(ApiURL, PBIAPI.Reports).ResponseToString());

                foreach (var item in objList.Items)
                {
                    item.ParentPowerBIAPI = this.ParentPowerBIAPI;
                    item.ParentObject = this;

                    if (!(this is PBIAPIClient)) // if the caller is a PBIClient, we do not have a ParentGroup but need to use "My Workspace" instead
                        item.ParentGroup = this;
                }

                return objList.Items;
            }
        }
    
        [JsonIgnore]
        public PBIAPIClient ParentPowerBIAPI { get; set; }

        [JsonIgnore]
        public PBIGroup ParentGroup { get; set; }
        [JsonIgnore]
        public string ApiURL
        {
            get
            {
                if(string.IsNullOrEmpty(_apiURL))
                    return string.Format("/v1.0/myorg/groups/{0}", Id);
                return _apiURL;
            }
            protected set { _apiURL = value; }
        }

        [JsonIgnore]
        public IPBIObject ParentObject { get; set; }


        public PBIReport GetReportByName(string name)
        {
            try
            {
                return Reports.Single(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (Exception e)
            {
                //return null;
                throw new KeyNotFoundException(string.Format("No Report with name '{0}' could be found in PowerBI!", name), e);
            }
        }

        public PBIReport GetReportByID(string id)
        {
            try
            {
                return Reports.Single(x => string.Equals(x.Id, id, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (Exception e)
            {
                //return null;
                throw new KeyNotFoundException(string.Format("No Report with ID '{0}' could be found in PowerBI!", id), e);
            }
        }
    }

    
}
