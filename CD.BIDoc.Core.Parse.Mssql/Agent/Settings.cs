//using CD.BIDoc.Core.Interfaces;
//using CD.Framework.Common.Interfaces;
//using Microsoft.SqlServer.Management.IntegrationServices;
//using Microsoft.SqlServer.Management.Smo.Agent;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CD.BIDoc.Core.Extract.Mssql.Agent
//{
//    /// <summary>
//    /// Settings for SSIS model extractor.
//    /// </summary>
//    public interface IAgentModelExtractSettings : IModelExtractSettings
//    {
//        /// <summary>
//        /// Filter specifying which projects should be extracted.
//        /// </summary>
//        IAgentJobFilter JobFilter { get; }
//    }

//    /// <summary>
//    /// Describes a filter of SSIS projects
//    /// </summary>
//    public interface IAgentJobFilter
//    {
//        /// <summary>
//        /// Gets the catalogs that should be extracted.
//        /// </summary>
//        IEnumerable<Tuple<Job, string>> EnumerateJobs(JobServer jobServer);
//    }



//    public class AgentJobFilter : IAgentJobFilter
//    {
//        private List<string> _names = new List<string>();

//        public AgentJobFilter(params string[] jobs):
//            this((IEnumerable<string>)jobs)
//        {
//        }

//        public AgentJobFilter(IEnumerable<string> jobs)
//        {
//            _names = jobs.ToList();
//        }

//        public void AddJob(string job)
//        {
//            _names.Add(job);
//        }

//        public IEnumerable<Tuple<Job, string>> EnumerateJobs(JobServer server)
//        {
//            return _names.Select(name => new Tuple<Job, string>(server.Jobs[name], name));
//        }
//    }
    


//}
