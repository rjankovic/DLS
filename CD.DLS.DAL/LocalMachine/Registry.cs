using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.LocalMachine
{
    public static class Registry
    {
        public const string CONFIG_REGISTRY_PATH = "SOFTWARE\\CleverDecision\\Framework";
        public const string CONFIG_REGISTRY_X86_PATH = "SOFTWARE\\WOW6432Node\\CleverDecision\\Framework";
        public const string CONFIG_AAD_INSTANCE_VALUE_NAME = "AADInstance";
        public const string CONFIG_TENANT_VALUE_NAME = "Tenant";
        public const string CONFIG_CLIENT_ID_VALUE_NAME = "ClientId";
        public const string CONFIG_REDIRECT_URI_VALUE_NAME = "RedirectUri";
        public const string CONFIG_GRAPH_RESOURCE_ID_VALUE_NAME = "GraphResourceId";
        public const string CONFIG_GRAPH_VERSION_VALUE_NAME = "GraphApiVersion";
        public const string CONFIG_GRAPH_ENDPOINT_VALUE_NAME = "GraphEndpoint";
        
        public static string ReadValueAsString(string keyPath, string value)
        {
            //try
            //{
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue(value);
                        if (o != null)
                        {
                            return o.ToString();
                        }
                    }
                }
            //}
            //catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
            //{
            //    //react appropriately
            //}

            return null;
        }

        public static void SetValueString(string keyPath, string valueName, string value)
        {
            RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath);
           
            if (key == null)
            {
                Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyPath);
            }
            key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, true);
            key.SetValue(valueName, value, RegistryValueKind.String);
        }

        public static string GetConfigValue(string valueName)
        {
            return ReadValueAsString(CONFIG_REGISTRY_PATH, valueName);
        }

        public static string GetConfigValue32(string valueName)
        {
            return ReadValueAsString(CONFIG_REGISTRY_X86_PATH, valueName);
        }

        public static void SetConfigValue(string valueName, string newValue)
        {
            SetValueString(CONFIG_REGISTRY_PATH, valueName, newValue);
        }

        public static void SetConfigValue6432(string valueName, string newValue)
        {
            SetValueString(CONFIG_REGISTRY_PATH, valueName, newValue);
            SetValueString(CONFIG_REGISTRY_X86_PATH, valueName, newValue);
        }
    }
}
