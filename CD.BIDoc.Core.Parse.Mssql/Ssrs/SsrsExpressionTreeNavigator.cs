using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace CD.DLS.Parse.Mssql.Ssas
{
    public class SsrsExpressionTreeNavigator : ParseTreeNavigator
    {
        public SsrsExpressionTreeNavigator(ParseTree tree)
            : base(tree)
        {
        }

        public enum ReferenceTypeEnum { Field, Parameter }
        public class PotentialReference
        {
            public ParseTreeNode ParseTreeNode { get; set; }
            public ReferenceTypeEnum ReferenceType { get; set; }
            public string Identifier { get; set; }
            public int ReferenceLength { get; set; }
        }

        private Dictionary<string, ReferenceTypeEnum> _dataItemRefTypeMap = new Dictionary<string, ReferenceTypeEnum>(StringComparer.OrdinalIgnoreCase)
        {
            { "Parameters", ReferenceTypeEnum.Parameter },
            { "Fields", ReferenceTypeEnum.Field }
        };

        public IEnumerable<PotentialReference> GetPotentialReferences(ParseTreeNode expressionSegment, string expressionText)
        {
            var dataItems = DFTraverseInner(expressionSegment).Where(x => x.Term.Name == "dataItemId");
            foreach (var dataItem in dataItems)
            {
                var idParts = DFTraverseInner(dataItem).Where(x => x.Term.Name == "idSimple").ToList();
                var firstPart = idParts[0].GetText(expressionText);
                if (_dataItemRefTypeMap.ContainsKey(firstPart))
                {
                    var refType = _dataItemRefTypeMap[firstPart];
                    var length = dataItem.Span.Length - (dataItem.Span.EndPosition - idParts[1].Span.EndPosition);
                    var secondPart = idParts[1].GetText(expressionText);

                    yield return new PotentialReference
                    {
                        ParseTreeNode = dataItem,
                        ReferenceType = refType,
                        Identifier = secondPart,
                        ReferenceLength = length
                    };
                }
            }
        }
    }   
}
