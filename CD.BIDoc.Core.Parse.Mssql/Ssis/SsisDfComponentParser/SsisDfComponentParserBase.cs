using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.DAL.Objects.Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.DAL.Configuration;

namespace CD.DLS.Parse.Mssql.Ssis.SsisDfComponentParser
{
    public abstract class SsisDfComponentParserBase
    {
        public string GetPropertyValueOrVariable(SsisDfComponent component, string key, SsisIndex referrables)
        {
            string value = component.GetPropertyValue(key);

            //string testValue;
            string accessMode = "";
            accessMode = component.GetPropertyValue("AccessMode");
            //if (!accessModeSpecified)
            //{
            //    accessMode = "";
            //}
            if (string.IsNullOrEmpty(accessMode))
            {
                accessMode = "";
            }

            if ((string.IsNullOrEmpty(component.GetPropertyValue(key)) //!properties.TryGetNonEmptyString(key, out value) 
                || !string.IsNullOrEmpty(component.GetPropertyValue(key + "Variable")) && (accessMode == "2" || accessMode == "4")))
            {
                var variableName = component.GetPropertyValue(key + "Variable"); // properties.GetString(key + "Variable");
                value = referrables.GetValueByName(variableName);
            }

            if (!string.IsNullOrEmpty(component.GetPropertyValue(key + "Variable")) && accessMode == "1")
            {
                var variableName = component.GetPropertyValue(key + "Variable"); // properties.GetString(key + "Variable");
                value = referrables.GetValueByName(variableName);
                ConfigManager.Log.Info(string.Format("Getting variable value for access mode 1 of {0}: {1}", component.ID, value));
            }

            return value;
        }
    }
}
