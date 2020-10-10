using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Ssis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Db = CD.DLS.Model.Mssql.Db;

namespace CD.DLS.Model.Mssql.Agent
{
    abstract public class AgentModelElement : MssqlModelElement
    {
        public AgentModelElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
           : base(refPath, caption)
        {
            Definition = definition;
            Parent = parent;
        }

        public AgentModelElement(RefPath refPath, string caption)
           : base(refPath, caption)
        { }
    }


    public class ServerElement : AgentModelElement
    {
        public ServerElement(RefPath refPath, string caption)
            : base(refPath, caption)
        { }

        public ServerElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
            : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<JobElement> Jobs { get { return ChildrenOfType<JobElement>(); } }
    }
    
    public class JobElement : AgentModelElement
    {
        public JobElement(RefPath refPath, string caption, string definition, ServerElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public StepElement StartStep { get; set; }

        public IEnumerable<StepElement> Steps { get { return ChildrenOfType<StepElement>(); } }
    }

    public abstract class StepElement : AgentModelElement
    {
        public StepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
        
        public OnStepSuccessElement OnSuccess { get { return ChildrenOfType<OnStepSuccessElement>().First(); } }
        public OnStepFailureElement OnFailure { get { return ChildrenOfType<OnStepFailureElement>().First(); } }

    }


    public class OnStepSuccessElement : AgentModelElement
    {
        public OnStepSuccessElement(RefPath refPath, string caption, string definition, StepElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public StepElement GoToStep { get; set; }
    }

    public class OnStepFailureElement : AgentModelElement
    {
        public OnStepFailureElement(RefPath refPath, string caption, string definition, StepElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public StepElement GoToStep { get; set; }
    }
    
    public class ActiveScriptingStepElement : StepElement
    {
        public ActiveScriptingStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class AnalysisCommandStepElement : StepElement
    {
        public AnalysisCommandStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class AnalysisQueryStepElement : StepElement
    {
        public AnalysisQueryStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class CmdExecStepElement : StepElement
    {
        public CmdExecStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class DistributionStepElement : StepElement
    {
        public DistributionStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class LogReaderStepElement : StepElement
    {
        public LogReaderStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class MergeStepElement : StepElement
    {
        public MergeStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class PowerShellStepElement : StepElement
    {
        public PowerShellStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class QueueReaderStepElement : StepElement
    {
        public QueueReaderStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class SnapshotStepElement : StepElement
    {
        public SnapshotStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class SsisStepElement : StepElement
    {
        public SsisStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public PackageElement SsisPackageElement { get; set; }
    }

    public class TsqlStepElement : StepElement
    {
        public TsqlStepElement(RefPath refPath, string caption, string definition, JobElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public Db.SqlScriptElement TsqlScriptElement { get { return ChildrenOfType<Db.SqlScriptElement>().First(); } }
    }
    
}
