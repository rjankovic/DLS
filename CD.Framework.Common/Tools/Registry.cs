using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Tools
{
    public static class Registry
    {
        public const string DB_CONNECTION_VALUE_NAME = "DbConnection";
        public const string CONFIG_REGISTRY_PATH = "SOFTWARE\\DLS";
        public const string CONFIG_REGISTRY_X86_PATH = "SOFTWARE\\WOW6432Node\\DLS";

        private static string ReadValueAsString(string keyPath, string value)
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

        private static void SetValueString(string keyPath, string valueName, string value)
        {
            RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath);

            if (key == null)
            {
                Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyPath);
            }
            key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, true);
            key.SetValue(valueName, value, RegistryValueKind.String);
        }

        public static string GetDbConnectionString()
        {
            return ReadValueAsString(CONFIG_REGISTRY_PATH, DB_CONNECTION_VALUE_NAME);
        }

        public static void SetDbConnectionString(string value)
        {
            SetValueString(CONFIG_REGISTRY_PATH, DB_CONNECTION_VALUE_NAME, value);
        }


        public static string GetConfigValue(string key)
        {
            return ReadValueAsString(CONFIG_REGISTRY_PATH, key);
        }

        public static void SetConfigValue(string key, string value)
        {
            SetValueString(CONFIG_REGISTRY_PATH, key, value);
        }

        public static string GetConfigValue32(string valueName)
        {
            return ReadValueAsString(CONFIG_REGISTRY_X86_PATH, valueName);
        }

        public static void SetConfigValue6432(string valueName, string newValue)
        {
            SetValueString(CONFIG_REGISTRY_PATH, valueName, newValue);
            SetValueString(CONFIG_REGISTRY_X86_PATH, valueName, newValue);
        }
    }
}
