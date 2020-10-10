using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Structures
{
    public enum DependencyGraphKind
    {
        DataFlow = 0,
        /// <summary>
        /// Based on the DataFlow graph, adds transitive links
        /// </summary>
        DataFlowTransitive = 1,
        /// <summary>
        /// High-level dataflow
        /// </summary>

        DataFlowMediumDetail = 2,
        DataFlowLowDetail = 3,
        DFHigh,
        ControlFlow,
        /// <summary>
        /// Based on the ControlFlow graph, adds transitive links
        /// </summary>
        ControlFlowTransitive,

    }
}
