using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using System.Collections.Generic;

namespace CD.DLS.Model
{
    /// <summary>
    /// Root model element containing other elements (SSIS, SQL, Agent, SSAS, SSRS, PBI)
    /// </summary>
    public class SolutionModelElement : MssqlModelElement
    {
        public SolutionModelElement(RefPath refPath, string caption)
           : base(refPath, caption)
        { }

        public IEnumerable<Mssql.Db.ServerElement> DbServers { get { return ChildrenOfType<Mssql.Db.ServerElement>(); } }
        public IEnumerable<Mssql.Ssas.ServerElement> SsasServers { get { return ChildrenOfType<Mssql.Ssas.ServerElement>(); } }
        public IEnumerable<Mssql.Ssis.ServerElement> SsisServers { get { return ChildrenOfType<Mssql.Ssis.ServerElement>(); } }
        public IEnumerable<Mssql.Ssrs.ServerElement> SsrsServers { get { return ChildrenOfType<Mssql.Ssrs.ServerElement>(); } }
        public IEnumerable<Mssql.Agent.ServerElement> AgentServers { get { return ChildrenOfType<Mssql.Agent.ServerElement>(); } }
        public IEnumerable<Mssql.Pbi.TenantElement> PbiTenants { get { return ChildrenOfType<Mssql.Pbi.TenantElement>(); } }
    }
    
}
