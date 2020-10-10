using CD.DLS.DAL.Azure;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace CD.DLS.DAL.Identity
{
    /// <summary>
    /// Provides login / logout and access to other features requiring an access token from AAD.
    /// </summary>
    public static class IdentityProvider
    {
        private static UserData _userData;
        private static AuthenticationContext _authContext = null;
        private static SecurityManager _securityManager = null;

        //static IdentityProvider()
        //{
        //    try
        //    {
        //        var fc = new FileCache();
        //        fc.Clear();
        //    }
        //    // if I'm a service, just leave it
        //    catch
        //    {

        //    }
        //}

        public static UserData GetCurrentUser()
        {
            return _userData;
        }

        /// <summary>
        /// Logs the user in, either via AAD or on-premises AD.
        /// </summary>
        public static void Login(bool saveToDb = true)
        {
            switch (Configuration.ConfigManager.DeploymentMode)
            {
                case Configuration.DeploymentModeEnum.OnPremises:
                    _securityManager = new SecurityManager();
                    LoginOnPremises(saveToDb);
                    break;
                case Configuration.DeploymentModeEnum.Azure:
                    LoginAad(saveToDb);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (_userData == null)
            {
                return;
            }

            if (_securityManager == null)
            {
                _securityManager = new SecurityManager();
            }
        }

        public static void Logout()
        {
            switch (Configuration.ConfigManager.DeploymentMode)
            {
                case Configuration.DeploymentModeEnum.OnPremises:
                    LogoutOnPremises();
                    break;
                case Configuration.DeploymentModeEnum.Azure:
                    LogoutAad();
                    break;
                default:
                    throw new NotImplementedException();
            }

            _userData = null;
        }

        private static void LogoutAad()
        {
            var fc = new FileCache(true);
            fc.Clear();
        }

        private static void LogoutOnPremises()
        {
        }

        /// <summary>
        /// Retrieves an Azure Key Vault secret from the customr's vault.
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public static string GetKeyVaultSecret(string secretName)
        {
            //try
            //{
                //"https://dlskeyvault.vault.azure.net/secrets/dlsqueuestoragecs/d0b0291939ab484abf32c6f7b4c200a0"

                //var azureServiceTokenProvider = new AzureServiceTokenProvider();

                //var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                //var scret = keyVaultClient.GetSecretAsync(keyvaultEndpoint, SecretName).GetAwaiter().GetResult();

                var kv = GetKeyVaultClient();
                var secret = Task.Run(async () => { try { return await kv.GetSecretAsync(ConfigManager.AzureKeyVaultBaseAddress, secretName); } catch (Exception ex) {
                        ConfigManager.Log.Error(string.Format("Could not get secret value {0}: {1}, {2}", secretName, ex.Message, ex.StackTrace));
                        throw new Exception(string.Format("Could not get secret value {0}: {1}, {2}", secretName, ex.Message, ex.StackTrace));
                    } }).Result; //kv.GetSecretAsync(ConfigManager.AzureKeyVaultBaseAddress, secretName);
                var value = secret.Value;
                if (value == null)
                {
                    if (value == null)
                    {
                        throw new Exception(string.Format("Could not get secret value {0}", secretName));
                    }
                }
                return value;
            //}
            //catch (AggregateException ex)
            //{
            //    if (ex.InnerException != null)
            //    {
            //        //MessageBox.Show(ex.InnerException.Message, "Connection error", MessageBoxButton.OK);
            //        return null;
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show("Azure is not available at the moment." + Environment.NewLine +"System Error: "+ ex.Message,"Connection error",MessageBoxButton.OK);
            //    return null;
            //}
                
        }

        /// <summary>
        /// Retrieves an Azure Key Vault secret from the customr's vault.
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public static string GetKeyVaultSecret(string secretName, string customerCode)
        {

            var kv = GetKeyVaultClient();
            var secret = Task.Run(async () => await kv.GetSecretAsync(ConfigManager.GetAzureKeyVaultBaseAddress(customerCode), secretName)).Result;
            //await kv.GetSecretAsync(ConfigManager.GetAzureKeyVaultBaseAddress(customerCode), secretName);
            var value = secret.Value;
            if (value == null)
            {
                throw new Exception(string.Format("Could not get secret value {0} for {1}", secretName, customerCode));
            }
            return value;
        }

        public static async Task<string> GetKeyVaultSecretVaultUrl(string vaultUrl, string secretName)
        {

            var kv = GetKeyVaultClient();
            var secret = await kv.GetSecretAsync(vaultUrl, secretName);
            var value = secret.Value;
            return value;
        }

        private static KeyVaultClient GetKeyVaultClient()
        {
            switch (Configuration.ConfigManager.ApplicationClass)
            {
                case ApplicationClassEnum.Client:
                    return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken));
                case ApplicationClassEnum.WebClient:
                    var azureServiceTokenProvider1 = new AzureServiceTokenProvider();
                    var keyVaultClient1 = new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(
                            azureServiceTokenProvider1.KeyVaultTokenCallback));
                    return keyVaultClient1;
                case ApplicationClassEnum.Service:
                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    var httpClient = new HttpClient();
                    return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback), httpClient);
                    //throw new Exception();
                default:
                    throw new NotImplementedException();

            }
        }

        public static List<Microsoft.Graph.Group> GetGroups()
        {
            var graphserviceClient = GetGraphServiceClient();
            
            var groups = Task.Run(async () => await graphserviceClient.Me.MemberOf.Request(new Option[] { }).GetAsync()).Result; 
            
            var res = new List<Group>();
            foreach (var grp in groups.OfType<Group>())
            {
                res.Add(grp);
            }


            //var users = GetCustGroupUsers();

            return res;
            
        }

        public static List<Microsoft.Graph.User> GetCustGroupUsers()
        {
            var graphserviceClient = GetGraphServiceClient();

            var groups = Task.Run(async () => await graphserviceClient.Me.MemberOf.Request(new Option[] { }).GetAsync()).Result;

            var res = new List<Group>();
            foreach (var grp in groups.OfType<Group>())
            {
                res.Add(grp);
            }

            var custGroup = res.First(x => x.DisplayName.StartsWith("cust"));

            //var users = graphserviceClient.Groups[custGroup.Id].Members.Request().GetAsync().Result;
            var users = Task.Run(async () => await graphserviceClient.Groups[custGroup.Id].Members.Request().GetAsync()).Result;
            List<User> userList = new List<User>();
            foreach (var user in users.OfType<User>())
            {
                userList.Add(user);
            }

            return userList;
        }

        private static GraphServiceClient GetGraphServiceClient()
        {
            var graphserviceClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        var access_token = _authContext.AcquireTokenSilentAsync(/*"https://graph.microsoft.com"*/ ConfigManager.MsGraphResourceId, ConfigManager.AadClientId).Result.AccessToken;
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", access_token);
                        return Task.FromResult(0);
                    }));

            return graphserviceClient;
        }
        

        private static void LoginOnPremises(bool saveToDb)
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (saveToDb)
            {
                _userData = _securityManager.GetOrCreateNewUser(windowsIdentity.Name, windowsIdentity.Name);
            }
            else
            {
                _userData = new UserData()
                {
                    Identity = windowsIdentity.Name,
                    DisplayName = windowsIdentity.Name
                };
            }
        }

        private static AuthenticationResult GetAadToken()
        {
            if (_authContext == null)
            {
                var authority = String.Format(CultureInfo.InvariantCulture, ConfigManager.AadInstance, ConfigManager.AzureTenant);

                if (ConfigManager.ApplicationClass == ApplicationClassEnum.WebClient)
                {
                    string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                    _authContext = new AuthenticationContext(authority, new ADALTokenCache(signedInUserID));
                }
                else
                {
                    _authContext = new AuthenticationContext(authority, new FileCache());
                }
            }

            AuthenticationResult authResult;
            try
            {
                if (ConfigManager.ApplicationClass == ApplicationClassEnum.WebClient)
                {
                    string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                    ClientCredential clientcred = new ClientCredential(ConfigManager.AadClientId, ConfigManager.AadClientSecret);

                    authResult = _authContext.AcquireTokenSilentAsync(ConfigManager.MsGraphResourceId, clientcred, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId)).Result;
                }
                else
                {
                    authResult = _authContext.AcquireTokenAsync(ConfigManager.MsGraphResourceId, ConfigManager.AadClientId, new Uri(ConfigManager.AadRedirectUri), new PlatformParameters(_userData == null ? PromptBehavior.SelectAccount : PromptBehavior.Auto)).Result;
                }
            }
            catch (AggregateException ex)
            {
                var exp = ex.InnerException;
                if (ex.InnerException.Message.Contains("AADSTS65005"))
                {
                    MessageBox.Show(string.Format("This account is not signed in DLS.", ex), "Signing problem", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return GetAadToken();
                }
                else if (ex.InnerException.Message.Contains("User canceled authentication"))
                {
                    //MessageBox.Show(string.Format("You canceled authentication. Please start app again.", ex), "Signing problem", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return null;
                }
                else if (ex.InnerException.Message.Contains("An error occurred while sending the request."))
                {
                    MessageBox.Show(string.Format("Internet access is interupted or Microsoft server is unavaible.", ex), "Signing problem", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    throw;
                }
                throw;
            }
            return authResult;
        }
        
        private static void LoginAad(bool saveToDb)
        {
            LogoutAad();
            var authResult = GetAadToken();
            if (authResult == null)
            {
                return;
            }
            var adGroups = GetGroups();
            var groups = adGroups.Select(x => new UserGroup() { Name = x.DisplayName, Id = x.Id }).ToList();
            var userData = new UserData()
            {
                Groups = groups,
                Identity = authResult.UserInfo.UniqueId,
                DisplayName = authResult.UserInfo.DisplayableId
            };

            _userData = userData;

            if (ConfigManager.ApplicationClass == ApplicationClassEnum.Client)
            {
                //var kvReaderPath = ConfigManager.KeyVaultReaderPath;
                //var secrets = RunProcess(kvReaderPath, new string[] { userData.GetCustomerCode() });
                //var secretSplit = secrets.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                //for (int i = 0; i < secretSplit.Length; i += 2)
                //{
                //    var key = secretSplit[i].Trim('\r');
                //    var value = secretSplit[i + 1].Trim('\r');
                //    ConfigManager.SetConfigValue(key, value);
                //}

            }

            if (saveToDb)
            {
                if (_securityManager == null)
                {
                    _securityManager = new SecurityManager();
                }
                var savedUser = _securityManager.GetOrCreateNewUser(userData.Identity, userData.DisplayName);
                userData.UserId = savedUser.UserId;
            }
            
        }

        private static async Task<string> GetToken(string authority, string resource, string scope)
        {
            if (_authContext == null)
            {
                if (ConfigManager.ApplicationClass == ApplicationClassEnum.WebClient)
                {
                    string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                    _authContext = new AuthenticationContext(authority, new ADALTokenCache(signedInUserID));
                }
                else
                {
                    _authContext = new AuthenticationContext(authority, new FileCache());
                }
            }

            AuthenticationResult result;

            /**/
            if (ConfigManager.ApplicationClass == ApplicationClassEnum.WebClient)
            {
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                ClientCredential clientcred = new ClientCredential(ConfigManager.AadClientId, ConfigManager.AadClientSecret);

                result = await _authContext.AcquireTokenSilentAsync(resource, clientcred, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
            }
            else
            {
            /**/
                result = await _authContext.AcquireTokenAsync(resource, ConfigManager.AadClientId, new Uri(ConfigManager.AadRedirectUri), new PlatformParameters(PromptBehavior.Auto));
            }

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

        /*
        private async Task<string> GetToken(string authority, string resource, string scope)
        {
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            ClientCredential clientcred = new ClientCredential(clientId, appKey);
            
            var context = new AuthenticationContext(authority, new ADALTokenCache(signedInUserID));
            
            AuthenticationResult result = await context.AcquireTokenSilentAsync(resource, clientcred, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }
         */

        private static string RunProcess(string fullPath, string[] args)
        {
            string stdOutput;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = fullPath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = System.IO.Path.GetTempPath(); // Path.GetDirectoryName(extractDirPath);
            startInfo.Arguments = string.Format(string.Join(" ", args));
            //ConfigManager.Log.Important("Executing " + fullPath, startInfo.Arguments);
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            using (System.Diagnostics.Process exeProcess = System.Diagnostics.Process.Start(startInfo))
            {
                stdOutput = exeProcess.StandardOutput.ReadToEnd();
                //ConfigManager.Log.Important(stdOutput);

                //var stdError = exeProcess.StandardError.ReadToEnd();
                //ConfigManager.Log.Error(stdError);


                //exeProcess.OutputDataReceived += ExeProcess_OutputDataReceived;
                //exeProcess.BeginOutputReadLine();
                //exeProcess.BeginErrorReadLine();

                exeProcess.WaitForExit();

                //exeProcess.OutputDataReceived -= ExeProcess_OutputDataReceived;

                if (exeProcess.ExitCode != 0)
                {
                    //ConfigManager.Log.Error("Extractor returned an unexpected exit code: " + exeProcess.ExitCode.ToString());
                    throw new Exception();
                }

            }

            return stdOutput;
        }

    }
}
