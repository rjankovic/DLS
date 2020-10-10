using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API
{

    public enum ElementTypeClassEnum
    {
        Field, Transformation, FieldAndTransformation
    }
    public class NodeDescription : NodeDeclaration
    {
        public string TypeDescription { get; set; }
        public string Definition { get; set; }
        public int TopologicalOrder { get; set; }
        public int DenseTopologicalOrder { get; set; }
        public ElementTypeClassEnum TypeClass { get; set; }
        public string DescriptivePath { get; set; }
    }

    public class VisualPartNodeDescription : NodeDescription
    {
        public int DefinitionOffset { get; set; }
        public int DefinitionLength { get; set; }
        public int VisualNodeId { get; set; }

        public VisualPartNodeDescription(NodeDescription desc)
        {
            this.Definition = desc.Definition;
            this.DenseTopologicalOrder = desc.DenseTopologicalOrder;
            this.ModelElementId = desc.ModelElementId;
            this.Name = desc.Name;
            this.NodeId = desc.NodeId;
            this.NodeType = desc.NodeType;
            this.RefPath = desc.RefPath;
            this.TopologicalOrder = desc.TopologicalOrder;
            this.TypeDescription = desc.TypeDescription;
            this.TypeClass = desc.TypeClass;
            this.DescriptivePath = desc.DescriptivePath;
        }

        public VisualPartNodeDescription()
        {
        }
    }

    public class VisualNodeDescription : NodeDescription
    {
        public int TextDefinitionOffset { get; set; }
        public int TextDefinitionLength { get; set; }
        public string TextDefinition { get; set; }
        public object VisualDefinition { get; set; }
        public VisualisationEnum VisualisationType { get; set; }
    }

    public enum VisualisationEnum { Html, Svg, Xaml, Text }
}
