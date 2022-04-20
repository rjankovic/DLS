using CD.DLS.Common.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace CD.DLS.Common.Structures
{
    public class ProjectConfig
    {
        public Guid ProjectConfigId { get; set; }
        public string Name { get; set; }
        public virtual List<MssqlDbProjectComponent> DatabaseComponents { get; set; }
        public virtual List<SsisProjectComponent> SsisComponents { get; set; }
        public virtual List<SsasDbProjectComponent> SsasComponents { get; set; }
        public virtual List<SsrsProjectComponent> SsrsComponents { get; set; }
        public virtual List<MssqlAgentProjectComponent> MssqlAgentComponents { get; set; }
        public virtual List<PowerBiProjectComponent> PowerBiComponents { get; set; }

        public ProjectConfig()
        {
            DatabaseComponents = new List<MssqlDbProjectComponent>();
            SsasComponents = new List<SsasDbProjectComponent>();
            SsisComponents = new List<SsisProjectComponent>();
            SsrsComponents = new List<SsrsProjectComponent>();
            MssqlAgentComponents = new List<MssqlAgentProjectComponent>();
            PowerBiComponents = new List<PowerBiProjectComponent>();
        }

        public string Explain()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Project {0}: {1}", ProjectConfigId, Name));
            foreach (var db in DatabaseComponents)
            {
                sb.AppendLine(string.Format("MSSQL DB {0} on {1}", db.DbName, db.ServerName));
            }
            foreach (var db in SsasComponents)
            {
                sb.AppendLine(string.Format("SSAS DB {0} on {1}", db.DbName, db.ServerName));
            }
            foreach (var ssis in SsisComponents)
            {
                sb.AppendLine(string.Format("SSIS project {0} on {1}", ssis.ProjectName, ssis.ServerName));
            }
            foreach (var ssrs in SsrsComponents)
            {
                sb.AppendLine(string.Format("SSRS folder {0} on {1}", ssrs.FolderPath, ssrs.ServerName));
            }
            foreach (var job in MssqlAgentComponents)
            {
                sb.AppendLine(string.Format("SQL Agent job {0} on {1}", job.JobName, job.ServerName));
            }

            return sb.ToString();
        }

        public string Serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static ProjectConfig Deserialize(string serialized)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject<ProjectConfig>(serialized, settings);
        }
    }
    public class MssqlDbProjectComponent
    {
        private string _serverName;

        public string ServerName { get { return _serverName; } set { _serverName = ConnectionStringTools.NormalizeServerName(value); } }
        public int MssqlDbProjectComponentId { get; set; }
        public string DbName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        [IgnoreDataMember]
        public virtual ProjectConfig ProjectConfig { get; set; }
    }

    public class SsisProjectComponent
    {
        private string _serverName;

        public string ServerName { get { return _serverName; } set { _serverName = ConnectionStringTools.NormalizeServerName(value); } }
        public int SsisProjectComponentId { get; set; }
        public string FolderName { get; set; }
        public string ProjectName { get; set; }
        [IgnoreDataMember]
        public virtual ProjectConfig ProjectConfig { get; set; }
    }

    public enum SsasTypeEnum
    {
        Multidimensional, Tabular
    };

    public class SsasDbProjectComponent
    {
        private string _serverName;

        public string ServerName { get { return _serverName; } set { _serverName = ConnectionStringTools.NormalizeServerName(value); } }
        public int SsaslDbProjectComponentId { get; set; }
        public string DbName { get; set; }
        [IgnoreDataMember]
        public virtual ProjectConfig ProjectConfig { get; set; }
        public SsasTypeEnum Type { get; set; }
    }


    public enum SsrsModeEnum { Native, SpIntegrated  }

    public class SsrsProjectComponent
    {
        private string _serverName;

        public string ServerName { get { return _serverName; } set { _serverName = ConnectionStringTools.NormalizeServerName(value); } }
        public string SsrsExecutionServiceUrl { get; set; }
        public string SsrsServiceUrl { get; set; }
        public int SsrsProjectComponentId { get; set; }
        public string FolderPath { get; set; }
        [IgnoreDataMember]
        public virtual ProjectConfig ProjectConfig { get; set; }
        public SsrsModeEnum SsrsMode { get; set; }
        public string SharePointBaseUrl { get; set; }
        public string SharePointFolder { get; set; }
        public string CombinedBaseUrl { get { return SsrsMode == SsrsModeEnum.Native ? SsrsServiceUrl : SharePointBaseUrl; } }
        public string CombinedFolder { get { return SsrsMode == SsrsModeEnum.Native ? FolderPath : SharePointFolder; } }
    }

    public class MssqlAgentProjectComponent
    {
        private string _serverName;

        public string ServerName { get { return _serverName; } set { _serverName = ConnectionStringTools.NormalizeServerName(value); } }
        public int MssqlAgentProjectComponentId { get; set; }
        public string JobName { get; set; }
        [IgnoreDataMember]
        public virtual ProjectConfig ProjectConfig { get; set; }
    }

    public enum PowerBiProjectConfigType { PbiAppDefaultWorkspace, PbiAppCustomWorkspace, ReportServer, DiskFolder }

    public class PowerBiProjectComponent
    {
        public int PowerBiProjectComponentId { get; set; }
        public PowerBiProjectConfigType ConfigType { get; set; }
        public string Tenant
        {
            get
            {
                switch (ConfigType)
                {
                    case PowerBiProjectConfigType.PbiAppCustomWorkspace:
                        return ApplicationID;
                    case PowerBiProjectConfigType.PbiAppDefaultWorkspace:
                        return ApplicationID;
                    case PowerBiProjectConfigType.DiskFolder:
                        return DiskFolder;
                    case PowerBiProjectConfigType.ReportServer:
                        return ReportServerURL;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        //public string Tenant { get; set; }
        public string RedirectUri { get; set; }
        public string ApplicationID { get; set; }
        public string WorkspaceID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ReportServerURL { get; set; }
        public string ReportServerFolder { get; set; }
        public string DiskFolder { get; set; }
        [IgnoreDataMember]
        public virtual ProjectConfig ProjectConfig { get; set; }
    }


    public class Credentials
    {
        public List<Credential> CredentialsList { get; set; }
        public Credentials()
        {
            CredentialsList = new List<Credential>();
        }

        public Credential FindCredential(int componentId, string componentType)
        {
            foreach (Credential cred in CredentialsList)
            {
                if (cred.ComponentId == componentId && cred.ComponentType == componentType)
                {
                    return cred;
                }
            }
            return null;
        }
    }

    public class Credential
    {
        public int ComponentId { get; set; }
        public string ComponentType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}