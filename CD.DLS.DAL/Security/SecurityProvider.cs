using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Security
{
    /// <summary>
    /// A GUI object or requestprocessor / other feature that needs permissions
    /// </summary>
    public static class SecurityProvider
    {
        private static SecurityManager _securityManager = null;

        private static Dictionary<string, Dictionary<PermissionEnum, Dictionary<string, Tuple<bool, string>>>> _permissionsCache = new Dictionary<string, Dictionary<PermissionEnum, Dictionary<string, Tuple<bool, string>>>>();

        private static bool? ResolveUsingCache(ISecuredObject securedObject, out string message)
        {
            message = null;
            var identity = IdentityProvider.GetCurrentUser().Identity;
            if (!_permissionsCache.ContainsKey(identity))
            {
                return null;
            }

            var userPermissions = _permissionsCache[identity];
            if (!userPermissions.ContainsKey(securedObject.RequiredPermission))
            {
                return null;
            }

            var argsPerms = userPermissions[securedObject.RequiredPermission];
            if (!argsPerms.ContainsKey(securedObject.PermissionScope))
            {
                return null;
            }

            var resp = argsPerms[securedObject.PermissionScope];
            message = resp.Item2;
            return resp.Item1;
        }

        private static void AddToCache(ISecuredObject securedObject, bool response, string message)
        {
            var identity = IdentityProvider.GetCurrentUser().Identity;
            if (!_permissionsCache.ContainsKey(identity))
            {
                _permissionsCache.Add(identity, new Dictionary<PermissionEnum, Dictionary<string, Tuple<bool, string>>>());
            }

            var userPermissions = _permissionsCache[identity];
            if (!userPermissions.ContainsKey(securedObject.RequiredPermission))
            {
                userPermissions.Add(securedObject.RequiredPermission, new Dictionary<string, Tuple<bool, string>>());
            }

            var argsPerms = userPermissions[securedObject.RequiredPermission];
            argsPerms[securedObject.PermissionScope] = new Tuple<bool, string>(response, message);
        }

        private static void ClearCache()
        {
            _permissionsCache = new Dictionary<string, Dictionary<PermissionEnum, Dictionary<string, Tuple<bool, string>>>>();
        }

        // GetSecurityQueryResponse(int userId, PermissionEnum permission, string scope)

        public static bool IsAllowed(ISecuredObject securedObject, out string message)
        {
            if (_securityManager == null)
            {
                _securityManager = new SecurityManager();
            }

            var userData = IdentityProvider.GetCurrentUser();

            var globalAdminGroup = userData.Groups.FirstOrDefault(x => x.Name == "GlobalAdmin");
            if (globalAdminGroup != null)
            {
                message = null;
                return true;
            }

            if (userData == null)
            {
                message = "The user has not logged in";
                return false;
            }

            var cacheResp = ResolveUsingCache(securedObject, out message);
            if (cacheResp.HasValue)
            {
                return cacheResp.Value;
            }

            var dbResp = _securityManager.GetSecurityQueryResponse(userData.UserId, securedObject.RequiredPermission, securedObject.PermissionScope);

            message = dbResp.Message;
            AddToCache(securedObject, dbResp.Result, message);
            return dbResp.Result;
        }
    }
}
