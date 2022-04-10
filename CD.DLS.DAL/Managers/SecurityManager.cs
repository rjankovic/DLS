using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.DAL.Security;
using CD.DLS.Common.Structures;

namespace CD.DLS.DAL.Managers
{
    public class RoleEventArgs : EventArgs
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
    public delegate void RoleEventHandler(object sender, RoleEventArgs e);


    public class SecurityManager
    {

        private NetBridge _netBridge;
        public NetBridge NetBridge { get { return _netBridge; } }
        public SecurityManager(NetBridge netBridge)
        {
            _netBridge = netBridge;
        }

        public SecurityManager()
        {
            _netBridge = new NetBridge();
        }

        public UserData GetOrCreateNewUser(string domainName, string displayName)
        {
            UserData res = new UserData();


            //find existing user
            var resDt = GetUsersData(domainName);

            //control
            if (resDt.Identity == null || resDt.DisplayName == null)
            {
                NetBridge.ExecuteProcedureTable("[Adm].[sp_AddUser]", new Dictionary<string, object>()
                {
                    { "userIdentity", domainName },
                    { "displayName", displayName}
                });
            }

            res = GetUsersData(domainName);

            return res;
        }

        public List<string> ListMembers(ProjectConfig projectConfig, string roleName)
        {
            var list = NetBridge.ExecuteProcedureTable("[Adm].[sp_GetDisplayNameAndRoleName]", new Dictionary<string, object>()
            {
                { "roleName", roleName}
            });
            List<string> res = new List<string>();
            foreach (DataRow r in list.Rows)
            {
                var memberNames = (string)r["DisplayName"];
                res.Add(memberNames);
            }
            return res;
        }

        public int GetId(string domainName)
        {
            int userId;
            var resDt = NetBridge.ExecuteTableFunction("[Adm].[f_GetUserDataByIdentity]", new Dictionary<string, object>()
                {
                    { "userIdentity", domainName }
                });
            userId = int.Parse(resDt.Rows[0][0].ToString());

            return userId;
        }

        private UserData ReadUsersData(DataTable table)
        {
            UserData res = new UserData();
            foreach (DataRow r in table.Rows)
            {
                var item = new UserData();

                item.UserId = (int)r["UserId"];
                item.Identity = (string)r["Identity"];
                item.DisplayName = r["DisplayName"] == DBNull.Value ? null : (string)r["DisplayName"];

                res = item;
            }
            return res;
        }

        public UserData GetUsersData(string domainName)
        {
            var wmDt = NetBridge.ExecuteTableFunction("[Adm].[f_GetUserDataByIdentity]", new Dictionary<string, object>()
            {
                { "userIdentity", domainName }
            });
            var dataFlow = ReadUsersData(wmDt);
            return dataFlow;
        }

        public SecurityQueryResponse GetSecurityQueryResponse(int userId, PermissionEnum permission, string scope)
        {
            /*
            CREATE PROCEDURE [Adm].[sp_SecurityQuery]
	@userId INT,
	@permission NVARCHAR(100),
	@scope NVARCHAR(MAX)
AS
             */
            var resDt = NetBridge.ExecuteProcedureTable("[Adm].[sp_SecurityQuery]", new Dictionary<string, object>()
            {
                { "userId", userId },
                { "permission", permission },
                { "scope", scope }
            });

            var res = ReadSecurityQueryResponse(resDt);
            return res;
        }

        public class SecurityQueryResponse
        {
            public bool Result { get; set; }
            public string Message { get; set; }
        }

        public enum SecurityRoleScope { Project, Global}

        public class SecurityRole
        {
            public string Name { get; set; }
            public int RoleId { get; set; }
            public string Member { get; set; }
            public string Permission { get; set; }
            public SecurityRoleScope Scope { get; set; }
        }

        public class SecurityRolePermission
        {
            public PermissionEnum Type { get; set; }
            public int PermissionId { get; set; }
            /*public string Scope { get; set; }*/
        }

        public class SecurityUser
        {
            public string DisplayName { get; set; }
            public int UserId { get; set; }
            public string Identity { get; set; }
        }

        private SecurityQueryResponse ReadSecurityQueryResponse(DataTable table)
        {
            SecurityQueryResponse res = new SecurityQueryResponse();
            DataRow r = table.Rows[0];

            res.Message = r["Message"] == DBNull.Value ? null : (string)r["Message"];
            res.Result = (bool)r["result"];
            return res;
        }

        public List<SecurityRole> ListRoles(ProjectConfig projectConfig)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Adm].[sp_ListRoles]", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfig.ProjectConfigId }
            });

            List<SecurityRole> res = new List<SecurityRole>();
            foreach (DataRow r in dt.Rows)
            {
                res.Add(new SecurityRole()
                {
                    Name = (string)r["RoleName"],
                    RoleId = (int)r["RoleId"],
                    Member = "Members",
                    Permission = "Permissions",
                    Scope = r["ProjectConfigId"] == DBNull.Value ? SecurityRoleScope.Global : SecurityRoleScope.Project
                });
            }

            return res;
        }

        public List<SecurityRolePermission> RolePermissions(int roleId)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Adm].[sp_GetRolePermissions]", new Dictionary<string, object>()
            {
                {   "roleId", roleId}
            });

            List<SecurityRolePermission> res = new List<SecurityRolePermission>();
            foreach (DataRow r in dt.Rows)
            {
                res.Add(new SecurityRolePermission()
                {
                    Type = (PermissionEnum)Enum.Parse(typeof(PermissionEnum), (string)r["PermissionName"]),
                    PermissionId = (int)r["PermissionId"]
                    /*Scope = (string)r["PermissionScope"]*/
                });
            }

            return res;
        }

        //public List<string>RolePermissionString(string roleName)
        //{
        //    var dt = NetBridge.ExecuteProcedureTable("[Adm].[sp_GetRolePermissions]", new Dictionary<string, object>()
        //    {
        //        {   "roleName", roleName}
        //    });

        //    List<string> res = new List<string>();
        //    foreach (DataRow r in dt.Rows)
        //    {
        //        res.Add(r.ItemArray.GetValue(0).ToString());
        //    }
        //    return res;
        //}

        public List<SecurityRolePermission> UserPermissions(Guid? projectConfigId)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Adm].[sp_GetUserPermissions]", new Dictionary<string, object>()
            {
                { "projectConfigId", (projectConfigId == null ? (Guid?)null : projectConfigId)},
                {   "userId", IdentityProvider.GetCurrentUser().UserId }
            });

            List<SecurityRolePermission> res = new List<SecurityRolePermission>();
            foreach (DataRow r in dt.Rows)
            {
                res.Add(new SecurityRolePermission()
                {
                    Type = (PermissionEnum)Enum.Parse(typeof(PermissionEnum), (string)r["PermissionName"]),
                    PermissionId = (int)r["PermissionId"]
                    /*Scope = (string)r["PermissionScope"]*/
                });
            }

            return res;
        }

        public List<SecurityRolePermission> ListPermissions()
        {
            var dt = NetBridge.ExecuteProcedureTable("[Adm].[sp_ListPermissions]", new Dictionary<string, object>()
            {
            });

            List<SecurityRolePermission> res = new List<SecurityRolePermission>();
            foreach (DataRow r in dt.Rows)
            {
                res.Add(new SecurityRolePermission()
                {
                    Type = (PermissionEnum)Enum.Parse(typeof(PermissionEnum), (string)r["PermissionName"]),
                    PermissionId = (int)r["PermissionId"]
                });
            }

            return res;
        }

        public List<SecurityUser> RoleMembers(int roleId)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Adm].[sp_GetRoleMembers]", new Dictionary<string, object>()
            {
                {   "roleId", roleId}
            });

            List<SecurityUser> res = new List<SecurityUser>();
            foreach (DataRow r in dt.Rows)
            {
                res.Add(new SecurityUser()
                {
                    DisplayName = (string)r["DisplayName"],
                    UserId = (int)r["UserId"]
                });
            }

            return res;
        }
        

        public DataTable AddRoleMember(int userId, int roleId)
        {
            var rs = NetBridge.ExecuteProcedureTable("[Adm].[sp_AddUserRoles]", new Dictionary<string, object>()
            {
                { "userId", userId},
                {   "roleId", roleId}
            });
            return rs;
        }

        //public List<string>Roles()
        //{
        //    var dt = NetBridge.ExecuteSelectStatement("SELECT RoleName FROM [Adm].[Roles]");

        //    List<string> res = new List<string>();
        //    foreach (DataRow r in dt.Rows)
        //    {
        //        res.Add(r.ItemArray.GetValue(0).ToString());
        //    }
        //    return res;
        //}

        public void AddRole(ProjectConfig projectConfig, string roleName)
        {
            NetBridge.ExecuteProcedure("[Adm].[sp_AddRole]", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfig.ProjectConfigId },
                { "roleName", roleName}
            });           
        }

        public void DeleteRole(int roleId)
        {
            NetBridge.ExecuteProcedure("[Adm].[sp_DeleteRole]", new Dictionary<string, object>()
            {
                { "roleId", roleId}
            });
        }

        public void DeleteRoleMember(int roleId, int userId)
        {
            NetBridge.ExecuteProcedure("[Adm].[sp_DeleteRoleMember]", new Dictionary<string, object>()
            {
                { "userId", userId},
                { "roleId", roleId}
            });
        }

        public void DeleteRolePermission(int roleId, int permissionId)
        {
            NetBridge.ExecuteProcedure("[Adm].[sp_DeleteRolePermission]", new Dictionary<string, object>()
            {
                { "roleId", roleId},
                { "permissionId", permissionId}
            });
        }

        //public List<string>Permissions()
        //{
        //    var dt = NetBridge.ExecuteSelectStatement("SELECT PermissionName FROM [Adm].[Permissions]");

        //    List<string> res = new List<string>();
        //    foreach (DataRow r in dt.Rows)
        //    {
        //        res.Add(r.ItemArray.GetValue(0).ToString());
        //    }
        //    return res;
        //}

        public List<SecurityUser> ListUsers()
        {
            //var dt = NetBridge.ExecuteSelectStatement("SELECT DisplayName FROM [Adm].[Users] u WHERE DisplayName IS NOT NULL");

            //List<string> res = new List<string>();
            //foreach (DataRow r in dt.Rows)
            //{
            //    res.Add(r.ItemArray.GetValue(0).ToString());
            //}
            //return res;

            var dt = NetBridge.ExecuteProcedureTable("[Adm].[sp_ListUsers]", new Dictionary<string, object>()
            {
            });

            List<SecurityUser> res = new List<SecurityUser>();
            foreach (DataRow r in dt.Rows)
            {
                res.Add(new SecurityUser()
                {
                    DisplayName = (string)r["DisplayName"],
                    UserId = (int)r["UserId"],
                    Identity = (string)r["Identity"]
                });
            }

            return res;
        }

        public void AddRolePermission(int roleId, int permissionId)
        {
            NetBridge.ExecuteProcedure("[Adm].[sp_AddRolePermission]", new Dictionary<string, object>()
            {
                { "roleId", roleId },
                { "permissionId", permissionId}
            });
        }

        public void AddRoleMemeber(int userId, int roleId)
        {
            NetBridge.ExecuteProcedure("[Adm].[sp_AddRoleMember]", new Dictionary<string, object>()
            {
                { "userId", userId },
                { "roleId", roleId }
            });
        }
    }
}