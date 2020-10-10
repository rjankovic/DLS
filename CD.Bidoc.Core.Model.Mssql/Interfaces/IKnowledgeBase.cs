using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CD.DLS.Model.Interfaces.DependencyGraph
{
    public interface IRuleApplicationContext
    {
        IDependencyGraphNode GetNode(IModelElement modelElement);
        IDependencyGraphNode GetNode(string refPath);
        void AddLink(IDependencyGraphNode fromNode, IDependencyGraphNode toNode, IRule rule);
        IDependencyGraph GetSourceGraphByKind(DependencyGraphKind kind);

    }

    public interface IRule
    {
        DependencyKind DependencyKind { get; }

        bool AppliesTo(IModelElement element, IRuleApplicationContext applicatoinContext);

        void Apply(IModelElement element, IRuleApplicationContext applicationContext);
    }
}
