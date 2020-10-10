using CD.DLS.Parse.Mssql.Agent;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Parse.Mssql.Ssas;
using CD.DLS.Parse.Mssql.Ssis;
using CD.DLS.Parse.Mssql.Ssrs;
using CD.DLS.Interfaces;
using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Storage.FileSystem;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Operations
{
    /// <summary>
    /// Addapts the BIDoc ILog to framework ILogger
    /// </summary>
    internal class Log : ILog
    {
        private ILogger _logger;

        public Log(ILogger logger)
        {
            this._logger = logger;
        }

        public void Error(string format, params object[] args)
        {
            _logger.Error(string.Format(format, args));
        }

        public void Warning(string format, params object[] args)
        {
            _logger.Warning(string.Format(format, args));
        }
    }

    //internal class ExtractSettingsProvider //: IExtractSettingsProvider
    //{
    //    private readonly ProjectConfig _projectConfig;
    //    private readonly ILogger _log;
    //    private readonly ProjectConfig _config;

    //    public ILogger Log
    //    {
    //        get { return _log; }
    //    }

    //    public ProjectConfig Config
    //    {
    //        get { return _config; }
    //    }



    //    //private class SsisSettings : ISsisModelExtractSettings
    //    //{
    //    //    private readonly ExtractSettingsProvider _provider;

    //    //    public SsisSettings(ExtractSettingsProvider provider)
    //    //    {
    //    //        _provider = provider;
    //    //    }

    //    //    public ILogger Log
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._log;
    //    //        }
    //    //    }

    //    //    public ISsisProjectFilter ProjectFilter
    //    //    {
    //    //        get
    //    //        {
    //    //            var ssisFiler = new SsisProjectFilter(Log,
    //    //                _provider._projectConfig.SsisComponents.Select(p => new SsisProjectInfo(p.FolderName, p.ProjectName))
    //    //                );
    //    //            ssisFiler.CatalogName = "SSISDB";
    //    //            return ssisFiler;
    //    //        }
    //    //    }

    //    //    public string ServerName
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._projectConfig.Server;
    //    //        }
    //    //    }

    //    //    public bool ExtractExpressions
    //    //    {
    //    //        get { return false; }
    //    //    }

    //    //    public ISsisPackageFilter PackageFilter
    //    //    {
    //    //        get { return new SsisAllPackagesFilter(); }
    //    //    }
    //    //}
    //    //private class DbSettings : IDbModelExtractSettings
    //    //{
    //    //    private readonly ExtractSettingsProvider _provider;

    //    //    public DbSettings(ExtractSettingsProvider provider)
    //    //    {
    //    //        _provider = provider;
    //    //    }

    //    //    public ILogger Log
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._log;
    //    //        }
    //    //    }

    //    //    public IDatabaseFilter DatabaseFilter
    //    //    {
    //    //        get
    //    //        {
    //    //            return new NameListDatabaseFilter(_provider._projectConfig.DatabaseComponents.Select(d => d.DbName));
    //    //        }
    //    //    }

    //    //    public string ServerName
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._projectConfig.Server;
    //    //        }
    //    //    }
    //    //}

    //    //private class SsasSettings : ISsasModelExtractSettings
    //    //{
    //    //    private readonly ExtractSettingsProvider _provider;

    //    //    public SsasSettings(ExtractSettingsProvider provider)
    //    //    {
    //    //        _provider = provider;
    //    //    }

    //    //    public ILogger Log
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._log;
    //    //        }
    //    //    }

    //    //    public ISsasDbFilter DbFilter
    //    //    {
    //    //        get
    //    //        {
    //    //            return new SsasDbFilter(_provider._projectConfig.SsasComponents.Select(x => x.DbName));
    //    //        }
    //    //    }

    //    //    public string ServerName
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._projectConfig.Server;
    //    //        }
    //    //    }
    //    //}

    //    //private class SsrsSettings : ISsrsModelExtractSettings
    //    //{
    //    //    private readonly ExtractSettingsProvider _provider;

    //    //    public SsrsSettings(ExtractSettingsProvider provider)
    //    //    {
    //    //        _provider = provider;
    //    //    }

    //    //    public ILogger Log
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._log;
    //    //        }
    //    //    }

    //    //    public ISsrsFolderFilter FolderFilter
    //    //    {
    //    //        get
    //    //        {
    //    //            return new SsrsFolderFilter(_provider._projectConfig.SsrsComponents.Select(x => x.FolderPath));
    //    //        }
    //    //    }

    //    //    public string ServerName
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._projectConfig.Server;
    //    //        }
    //    //    }

    //    //    public string ServiceUrl
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._projectConfig.SsrsServiceUrl;
    //    //        }
    //    //    }

    //    //    public string ExecutionServiceUrl
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._projectConfig.SsrsExecutionServiceUrl;
    //    //        }
    //    //    }
    //    //}

    //    //private class AgentSettings : IAgentModelExtractSettings
    //    //{
    //    //    private readonly ExtractSettingsProvider _provider;

    //    //    public AgentSettings(ExtractSettingsProvider provider)
    //    //    {
    //    //        _provider = provider;
    //    //    }

    //    //    public ILogger Log
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._log;
    //    //        }
    //    //    }

    //    //    public IAgentJobFilter JobFilter
    //    //    {
    //    //        get
    //    //        {
    //    //            return new AgentJobFilter(_provider._projectConfig.MssqlAgentComponents.Select(x => x.JobName));
    //    //        }
    //    //    }

    //    //    public string ServerName
    //    //    {
    //    //        get
    //    //        {
    //    //            return _provider._projectConfig.Server;
    //    //        }
    //    //    }
    //    //}

    //    //public T GetSettings<T>() where T : class, IModelExtractSettings
    //    //{
    //    //    if (typeof(T) == typeof(ISsisModelExtractSettings))
    //    //    {
    //    //        if (_projectConfig.SsisComponents.Count == 0)
    //    //        {
    //    //            return null;
    //    //        }
    //    //        return (T)(ISsisModelExtractSettings)new SsisSettings(this);
    //    //    }
    //    //    else if (typeof(T) == typeof(IDbModelExtractSettings))
    //    //    {
    //    //        if (_projectConfig.DatabaseComponents.Count == 0)
    //    //        {
    //    //            return null;
    //    //        }
    //    //        return (T)(IDbModelExtractSettings)new DbSettings(this);
    //    //    }
    //    //    else if (typeof(T) == typeof(ISsasModelExtractSettings))
    //    //    {
    //    //        if (_projectConfig.SsasComponents.Count == 0)
    //    //        {
    //    //            return null;
    //    //        }
    //    //        return (T)(ISsasModelExtractSettings)new SsasSettings(this);
    //    //    }
    //    //    else if (typeof(T) == typeof(ISsrsModelExtractSettings))
    //    //    {
    //    //        if (_projectConfig.SsrsComponents.Count == 0)
    //    //        {
    //    //            return null;
    //    //        }
    //    //        return (T)(ISsrsModelExtractSettings)new SsrsSettings(this);
    //    //    }
    //    //    else if (typeof(T) == typeof(IAgentModelExtractSettings))
    //    //    {
    //    //        if (_projectConfig.MssqlAgentComponents.Count == 0)
    //    //        {
    //    //            return null;
    //    //        }
    //    //        return (T)(IAgentModelExtractSettings)new AgentSettings(this);
    //    //    }
    //    //    else
    //    //        return null;
    //    //}
    //    public ExtractSettingsProvider(ProjectConfig projectConfig, ILogger logger)
    //    {
    //        _projectConfig = projectConfig;
    //        _log = logger; // new Log(logger);
    //    }
    //}

}
