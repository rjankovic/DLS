using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Identity
{
    public class WebIdentityProvider
    {
        public async Task<UserData> GetAadUser()
        {
            string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            var graphResourceID = "https://graph.windows.net";
            Uri servicePointUri = new Uri(graphResourceID);
            Uri serviceRoot = new Uri(servicePointUri, tenantID);
            ActiveDirectoryClient activeDirectoryClient = new ActiveDirectoryClient(serviceRoot,
                  async () => await GetTokenForApplication());

            // use the token for querying the graph to get the user details

            var result = await activeDirectoryClient.Users
                .Where(u => u.ObjectId.Equals(userObjectID))
                .ExecuteAsync();

            IUser user = result.CurrentPage.ToList().First();

            var userGroupsResult = await user.GetMemberGroupsAsync(true);
            var userGroups = userGroupsResult.ToList();

            List<UserGroup> groups = new List<UserGroup>();

            foreach (var groupId in userGroups)
            {
                var groupResult = await activeDirectoryClient.Groups.GetByObjectId(groupId).ExecuteAsync();
                groups.Add(new UserGroup() { Id = groupResult.ObjectId, Name = groupResult.DisplayName });
            }

            var userData = new UserData()
            {
                DisplayName = user.DisplayName,
                Identity = user.ObjectId,
                Groups = groups
            };
            
            return userData;
        }

        public void SaveUserData(UserData userData, SecurityManager securityManager)
        {
            var savedUser = securityManager.GetOrCreateNewUser(userData.Identity, userData.DisplayName);
            userData.UserId = savedUser.UserId;
        }

        public async Task<string> GetTokenForApplication()
        {
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            // get a token for the Graph without triggering any user interaction (from the cache, via multi-resource refresh token, etc)
            ClientCredential clientcred = new ClientCredential(ConfigManager.AadInstance, ConfigManager.AadClientSecret);
            // initialize AuthenticationContext with the token cache of the currently signed in user, as kept in the app's database
            AuthenticationContext authenticationContext = new AuthenticationContext(ConfigManager.AadInstance + tenantID, new ADALTokenCache(signedInUserID));
            //ConfigManager.AadClientId, new Uri(ConfigManager.AadRedirectUri), new PlatformParameters(PromptBehavior.Auto)
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync(ConfigManager.MsGraphResourceId, ConfigManager.AadClientId, new Uri(ConfigManager.AadRedirectUri), new PlatformParameters(PromptBehavior.Auto));
            //AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenSilentAsync(ConfigManager.MsGraphResourceId, clientcred, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
            return authenticationResult.AccessToken;
        }
    }
}
