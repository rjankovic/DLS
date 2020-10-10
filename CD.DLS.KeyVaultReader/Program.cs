using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.KeyVaultReader
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var customerCode = args[0];
                Console.WriteLine(StandardConfigManager.KV_DLS_ASB_CUSTOMER_SUBSCRIPTION);
                Console.WriteLine(IdentityProvider.GetKeyVaultSecret(StandardConfigManager.KV_DLS_ASB_CUSTOMER_SUBSCRIPTION, "cpi"));
                Console.WriteLine(StandardConfigManager.KV_DLS_ASB_CUSTOMER_TOPIC);
                Console.WriteLine(IdentityProvider.GetKeyVaultSecret(StandardConfigManager.KV_DLS_ASB_CUSTOMER_TOPIC, "cpi"));
                Console.WriteLine(StandardConfigManager.KV_DLS_CUSTOMER_CONNECTION_STRING);
                Console.WriteLine(IdentityProvider.GetKeyVaultSecret(StandardConfigManager.KV_DLS_CUSTOMER_CONNECTION_STRING, "cpi"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
                throw;
            }
        }
    }
}
