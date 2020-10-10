using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Structures
{
    // ugly copy of dependency kind
    public enum LinkTypeEnum
    {
        /// <summary>
        /// The meaning of the dependency is not known.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The target node is a parent of the source node.
        /// </summary>
        Parent = 1,
        /// <summary>
        /// The target node is a child of the source node.
        /// </summary>
        Child = 2,
        /// <summary>
        /// The definition of the source node depends on (uses) the existence of the target node.
        /// </summary>
        DefinitionDepends = 3,
        /// <summary>
        /// The definition of the target node depends on (uses) the existence of the source node.
        /// </summary>
        DefinitionDependent = 4,

        DataFlow = 5,
        /// <summary>
        /// The target node is a source of a foreign key
        /// </summary>
        ForeignKeySource = 6,
        /// <summary>
        /// The target node is a target of a foreign key
        /// </summary>
        ForeignKeyTarget = 7,

        ControlFlowPredecessor = 8,
        ControlFlowSuccessor = 9,


        /// <summary>
        /// The target's definition contains the source's definition
        /// </summary>
        DefinitionContainer = 10,

    }

}
