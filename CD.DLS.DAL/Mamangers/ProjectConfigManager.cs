using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Managers
{
    public class ProjectConfigManager
    {
        public const string CONFIG_KEY_SVC_RUNCONSOLE = "SVC_RUNCONSOLE";
        public const string CONFIG_KEY_SVC_INSTANCE_ID = "SVC_INSTANCE_ID";

        private NetBridge _netBridge;
        public NetBridge NetBridge { get { return _netBridge; } }
        public ProjectConfigManager(NetBridge netBridge)
        {
            _netBridge = netBridge;
        }

        public ProjectConfigManager()
        {
            _netBridge = new NetBridge();
        }

        public ProjectConfig GetProjectConfig(Guid id)
        {

            var configNameTable = NetBridge.ExecuteTableFunction("Adm.f_GetProjectConfigName", new Dictionary<string, object>()
            {
                { "projectconfigid", id}
            });
            var projName = (string)configNameTable.Rows[0][0];
            ProjectConfig res = new ProjectConfig()
            {
                ProjectConfigId = id,
                Name = projName,
                MssqlAgentComponents = new List<MssqlAgentProjectComponent>(),
                DatabaseComponents = new List<MssqlDbProjectComponent>(),
                SsasComponents = new List<SsasDbProjectComponent>(),
                SsisComponents = new List<SsisProjectComponent>(),
                SsrsComponents = new List<SsrsProjectComponent>(),
                PowerBiComponents = new List<PowerBiProjectComponent>()
            };


            Credentials credentials = null;
            if (System.IO.Path.GetDirectoryName(ConfigManager.ExtractorPath) != null)
            {
                var credentialsFilePath = Path.Combine(System.IO.Path.GetDirectoryName(ConfigManager.ExtractorPath), "config", res.ProjectConfigId.ToString());
                Directory.CreateDirectory(Path.GetDirectoryName(credentialsFilePath));

                if (File.Exists(credentialsFilePath))
                {
                    credentials = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText(credentialsFilePath));
                }
            }
               

            var mssqlDbTable = NetBridge.ExecuteTableFunction("Adm.f_GetMssqlDbProjectComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", id}
            });
            var agentTable = NetBridge.ExecuteTableFunction("Adm.f_GetMssqlAgentProjectComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", id}
            });
            var ssasTable = NetBridge.ExecuteTableFunction("Adm.f_GetSsasDbProjectComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", id}
            });
            var ssisTable = NetBridge.ExecuteTableFunction("Adm.f_GetSsisProjectComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", id}
            });
            var ssrsTable = NetBridge.ExecuteTableFunction("Adm.f_GetSsrsProjectComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", id}
            });
            var powerBiTable = NetBridge.ExecuteTableFunction("Adm.f_GetPowerBiProjectComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", id}
            });

            foreach (DataRow row in mssqlDbTable.Rows)
            {
                Credential credential = credentials != null ? credentials.FindCredential((int)row["MssqlDbProjectComponentId"], "Mssql") : null;
                res.DatabaseComponents.Add(new MssqlDbProjectComponent()
                {
                    ServerName = (string)row["ServerName"],
                    DbName = (string)row["DbName"],
                    MssqlDbProjectComponentId = (int)row["MssqlDbProjectComponentId"],
                    Username = credential != null ? credential.Username : "",
                    Password = credential != null ? credential.Password : "",
                    ProjectConfig = res
                });
            }           
            foreach (DataRow row in agentTable.Rows)
            {
                res.MssqlAgentComponents.Add(new MssqlAgentProjectComponent()
                {
                    ServerName = (string)row["ServerName"],
                    JobName = (string)row["JobName"],
                    MssqlAgentProjectComponentId = (int)row["MssqlAgentProjectComponentId"],
                    ProjectConfig = res
                });
            }
            foreach (DataRow row in ssasTable.Rows)
            {
                //ConfigManager.Log.Important("ProjectConfigManager Tabular/OLAP type "+ (string)row["SSASType"]);
                SsasTypeEnum st;
                if (((string)row["SSASType"]).Equals("Tabular"))
                {
                    st = SsasTypeEnum.Tabular;
                }
                else
                {
                    st = SsasTypeEnum.Multidimensional;
                }

                res.SsasComponents.Add(new SsasDbProjectComponent()
                {
                    ServerName = (string)row["ServerName"],
                    DbName = (string)row["DbName"],
                    SsaslDbProjectComponentId = (int)row["SsaslDbProjectComponentId"],
                    Type = st,
                    ProjectConfig = res
                });
            }
            foreach (DataRow row in ssisTable.Rows)
            {
                res.SsisComponents.Add(new SsisProjectComponent()
                {
                    ServerName = (string)row["ServerName"],
                    FolderName = (string)row["FolderName"],
                    ProjectName = (string)row["ProjectName"],
                    SsisProjectComponentId = (int)row["SsisProjectComponentId"],
                    ProjectConfig = res 
                });
            }
            foreach (DataRow row in ssrsTable.Rows)
            {
                res.SsrsComponents.Add(new SsrsProjectComponent()
                {
                    SsrsMode = (SsrsModeEnum)Enum.Parse(typeof(SsrsModeEnum), (string)row["SsrsMode"]),
                    ServerName = (string)row["ServerName"],
                    FolderPath = row["FolderPath"] == DBNull.Value ? null : (string)row["FolderPath"],
                    SsrsServiceUrl = row["SsrsServiceUrl"] == DBNull.Value ? null : (string)row["SsrsServiceUrl"],
                    SsrsExecutionServiceUrl = row["SsrsExecutionServiceUrl"] == DBNull.Value ? null : (string)row["SsrsExecutionServiceUrl"],
                    SsrsProjectComponentId = (int)row["SsrsProjectComponentId"],
                    SharePointBaseUrl = row["SharepointBaseUrl"] == DBNull.Value ? null : (string)row["SharepointBaseUrl"],
                    SharePointFolder = row["SharepointFolder"] == DBNull.Value ? null : (string)row["SharepointFolder"],
                    ProjectConfig = res
                });
            }
           
            foreach (DataRow row in powerBiTable.Rows)
            {                              
                Credential credential = credentials != null ? credentials.FindCredential((int)row["PowerBiProjectComponentId"], "PowerBi") : null;

                res.PowerBiComponents.Add(new PowerBiProjectComponent()
                {
                    RedirectUri = (string)row["RedirectUri"],
                    ApplicationID = (string)row["ApplicationId"],
                    WorkspaceID = row["WorkspaceID"] == DBNull.Value ? null : (string)row["WorkspaceID"],
                    PowerBiProjectComponentId = (int)row["PowerBiProjectComponentId"],
                    UserName = credentials != null ? credential.Username : "",
                    Password = credentials != null ? credential.Password : "",
                    Tenant = (string)row["ApplicationId"],
                    ReportServerURL = row["ReportServerURL"] == DBNull.Value ? null : (string)row["ReportServerURL"],
                    ReportServerFolder = row["ReportServerFolder"] == DBNull.Value ? null : (string)row["ReportServerFolder"],
                    ProjectConfig = res
                });

            }

            return res;
        }

        public Guid GetServiceReceiverId()
        {
            var res = NetBridge.ExecuteProcedureScalar("[Adm].[sp_GetGlobalConfigValue]", new Dictionary<string, object>()
            {
                { "key", CONFIG_KEY_SVC_INSTANCE_ID }
            });

            return Guid.Parse((string)res);
        }

        public List<ProjectConfig> ListProjectConfigs()
        {
            var list = NetBridge.ExecuteSelectStatement("SELECT * FROM Adm.ProjectConfigs");
            List<ProjectConfig> res = new List<ProjectConfig>();
            foreach (DataRow r in list.Rows)
            {
                var projectId = (Guid)r["ProjectConfigId"];
                var project = GetProjectConfig(projectId);
                res.Add(project
                    //new ProjectConfig { Name = (string)r["Name"], ProjectConfigId = (Guid)r["ProjectConfigId"] }
                    );
            }
            return res;
        }


        public void SaveProjectConfig(ProjectConfig config, bool createNew)
        {
            if (createNew)
            {
                DeleteProjectConfig(config.ProjectConfigId);
                NetBridge.ExecuteProcedure("Adm.sp_CreateProjectConfig", new Dictionary<string, object>()
                {
                    { "projectconfigid", config.ProjectConfigId },
                    { "name", config.Name }
                });
                NetBridge.ExecuteProcedure("Annotate.sp_CreateViews", new Dictionary<string, object>()
                {
                    { "projectconfigid", config.ProjectConfigId }
                });
                NetBridge.ExecuteProcedure("Adm.sp_CreateProjectRoles", new Dictionary<string, object>()
                {
                    { "projectconfigid", config.ProjectConfigId }
                });
            }

            DataTable mssqlDbComponents = new DataTable();
            mssqlDbComponents.TableName = "Adm.UDTT_MssqlDbProjectComponents";
            mssqlDbComponents.Columns.Add("ServerName", typeof(string));
            mssqlDbComponents.Columns.Add("DbName", typeof(string));
            foreach (var mssqlDb in config.DatabaseComponents)
            {              
                var nr = mssqlDbComponents.NewRow();
                nr[0] = mssqlDb.ServerName;
                nr[1] = mssqlDb.DbName;
                mssqlDbComponents.Rows.Add(nr);
            }

            DataTable ssasDbComponents = new DataTable();
            ssasDbComponents.TableName = "Adm.UDTT_SsasDbProjectComponents";
            ssasDbComponents.Columns.Add("ServerName", typeof(string));
            ssasDbComponents.Columns.Add("DbName", typeof(string));
            ssasDbComponents.Columns.Add("SSASType", typeof(string));
            foreach (var ssasDb in config.SsasComponents)
            {
                var nr = ssasDbComponents.NewRow();
                nr[0] = ssasDb.ServerName;
                nr[1] = ssasDb.DbName;
                nr[2] = ssasDb.Type;
                ssasDbComponents.Rows.Add(nr);
            }

            DataTable ssisComponents = new DataTable();
            ssisComponents.TableName = "Adm.UDTT_SsisProjectComponents";
            ssisComponents.Columns.Add("ServerName", typeof(string));
            ssisComponents.Columns.Add("FolderName", typeof(string));
            ssisComponents.Columns.Add("ProjectName", typeof(string));
            foreach (var ssis in config.SsisComponents)
            {
                var nr = ssisComponents.NewRow();
                nr[0] = ssis.ServerName;
                nr[1] = ssis.FolderName;
                nr[2] = ssis.ProjectName;
                ssisComponents.Rows.Add(nr);
            }

            DataTable agentComponents = new DataTable();
            agentComponents.TableName = "Adm.UDTT_MssqlAgentProjectComponents";
            agentComponents.Columns.Add("ServerName", typeof(string));
            agentComponents.Columns.Add("JobName", typeof(string));
            foreach (var agentJob in config.MssqlAgentComponents)
            {
                var nr = agentComponents.NewRow();
                nr[0] = agentJob.ServerName;
                nr[1] = agentJob.JobName;
                agentComponents.Rows.Add(nr);
            }

            DataTable ssrsComponents = new DataTable();
            ssrsComponents.TableName = "Adm.UDTT_SsrsProjectComponents";
            ssrsComponents.Columns.Add("SsrsMode", typeof(string));
            ssrsComponents.Columns.Add("ServerName", typeof(string));
            ssrsComponents.Columns.Add("SsrsServiceUrl", typeof(string));
            ssrsComponents.Columns.Add("SsrsExecutionServiceUrl", typeof(string));
            ssrsComponents.Columns.Add("FolderPath", typeof(string));
            ssrsComponents.Columns.Add("SharepointBaseUrl", typeof(string));
            ssrsComponents.Columns.Add("SharepointFolder", typeof(string));
      
            foreach (var ssrs in config.SsrsComponents)
            {
                var nr = ssrsComponents.NewRow();
                nr[0] = ssrs.SsrsMode.ToString();
                nr[1] = ssrs.ServerName;
                nr[2] = ssrs.SsrsServiceUrl;
                nr[3] = ssrs.SsrsExecutionServiceUrl;
                nr[4] = ssrs.FolderPath;
                nr[5] = ssrs.SharePointBaseUrl;
                nr[6] = ssrs.SharePointFolder;
                ssrsComponents.Rows.Add(nr);
            }

            DataTable powerBiComponents = new DataTable();
            powerBiComponents.TableName = "Adm.UDTT_PowerBiProjectComponents";
            powerBiComponents.Columns.Add("RedirectUri", typeof(string));
            powerBiComponents.Columns.Add("ApplicationID", typeof(string));
            powerBiComponents.Columns.Add("WorkspaceID", typeof(string));
            powerBiComponents.Columns.Add("ReportServerURL", typeof(string));
            powerBiComponents.Columns.Add("ReportServerFolder", typeof(string));
            foreach (var powerBi in config.PowerBiComponents)
            {
                var nr = powerBiComponents.NewRow();
                nr[0] = powerBi.RedirectUri;
                nr[1] = powerBi.ApplicationID;
                nr[2] = powerBi.WorkspaceID;
                nr[3] = powerBi.ReportServerURL;
                nr[4] = powerBi.ReportServerFolder;
                powerBiComponents.Rows.Add(nr);
            }

            NetBridge.ExecuteProcedure("Adm.sp_SetProjectMssqlAgentComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", config.ProjectConfigId },
                { "components", agentComponents }
            });
            NetBridge.ExecuteProcedure("Adm.sp_SetProjectMssqlDbComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", config.ProjectConfigId },
                { "components", mssqlDbComponents }
            });
            NetBridge.ExecuteProcedure("Adm.sp_SetProjectSsasDbComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", config.ProjectConfigId },
                { "components", ssasDbComponents }
            });
            NetBridge.ExecuteProcedure("Adm.sp_SetProjectSsisComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", config.ProjectConfigId },
                { "components", ssisComponents }
            });
            NetBridge.ExecuteProcedure("Adm.sp_SetProjectSsrsComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", config.ProjectConfigId },
                { "components", ssrsComponents }
            });
            NetBridge.ExecuteProcedure("Adm.sp_SetProjectPowerBiComponents", new Dictionary<string, object>()
            {
                { "projectconfigid", config.ProjectConfigId },
                { "components", powerBiComponents }
            });


            var credentialsFilePath = Path.Combine(System.IO.Path.GetDirectoryName(ConfigManager.ExtractorPath), "config", config.ProjectConfigId.ToString());
            File.Delete(credentialsFilePath);
            Credentials credentials = new Credentials();

            var projectConfig = this.GetProjectConfig(config.ProjectConfigId);
           
            for (int i = 0 ; i < projectConfig.PowerBiComponents.Count;i++)
            {
                Credential credential = new Credential()
                {
                    ComponentId = projectConfig.PowerBiComponents[i].PowerBiProjectComponentId,                    
                    ComponentType = "PowerBi",
                    Username = config.PowerBiComponents[i].UserName,
                    Password = config.PowerBiComponents[i].Password                   
                };
                credentials.CredentialsList.Add(credential);
            }

            for (int i = 0; i < projectConfig.DatabaseComponents.Count; i++)
            {
                Credential credential = new Credential()
                {
                    ComponentId = projectConfig.DatabaseComponents[i].MssqlDbProjectComponentId,
                    ComponentType = "Mssql",
                    Username = config.DatabaseComponents[i].Username,
                    Password = config.DatabaseComponents[i].Password
                };
                credentials.CredentialsList.Add(credential);
            }

            File.WriteAllText(credentialsFilePath, JsonConvert.SerializeObject(credentials));
        }

        public void DeleteProjectConfig(Guid id)
        {
            var credentialsFilePath = Path.Combine(System.IO.Path.GetDirectoryName(ConfigManager.ExtractorPath),"config",id.ToString());
            
            NetBridge.ExecuteProcedure("Adm.sp_DeleteProjectConfig", new Dictionary<string, object>()
            {
                { "projectconfigid", id }
            });
        }

        // sp_CreateNewProjectSchemaAndTables
        // @projectconfigid UNIQUEIDENTIFIER
        
        public void CreateNewProjectSchemaAndTables(Guid projectId)
        {
            NetBridge.ExecuteProcedure("Adm.sp_CreateNewProjectSchemaAndTables", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId }
            });
        }

        public void CreateProjectViews()
        {
            NetBridge.ExecuteProcedure("Adm.sp_CreateProjectViews", new Dictionary<string, object>()
            {
            });
        }


        public void CreateDataflowSequences()
        {
            // [Adm].[sp_CreateDataflowSequences]

            NetBridge.ExecuteProcedure("[Adm].[sp_CreateDataflowSequences]", new Dictionary<string, object>()
            {
            });
        }

        //public void SetGlobalConfigValue(string key, string value)
        //{
        //    NetBridge.ExecuteProcedure("Adm.sp_SetGlobalConfigValue", new Dictionary<string, object>()
        //    {
        //        { "key", key },
        //        { "value", value }
        //    });
        //}

        //public string GetGlobalConfigValue(string key)
        //{
        //    var res = NetBridge.ExecuteProcedureTable("Adm.sp_GetGlobalConfigValue", new Dictionary<string, object>()
        //    {
        //        { "key", key }
        //    });

        //    return (string)(res.Rows[0][0]);
        //}
    }
}
