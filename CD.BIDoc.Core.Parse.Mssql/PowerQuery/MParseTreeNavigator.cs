using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace CD.DLS.Parse.Mssql.PowerQuery
{
    public class MParseTreeNavigator : ParseTreeNavigator
    {
        public MParseTreeNavigator(ParseTree tree)
            :base(tree)
        {
        }

        public string Script { get { return _tree.SourceText; } }
        
        public ParseTreeNode FindExpressionNode()
        {
            return DFTraverse(_tree).FirstOrDefault(x => x != null && x.Term.Name == MGrammar. NONTERM_EXPRESSION);
        }
        
        public ParseTreeNode FindExpressionNode(ParseTreeNode root)
        {
            return DFTraverseInner(root).FirstOrDefault(x => x != null && x.Term.Name == DaxGrammar.NONTERM_EXPRESSION);
        }

        public IEnumerable<ParseTreeNode> GetLocalDefinitions()
        {
            return DFTraverse(_tree).Where(x => x.Term.Name == DaxGrammar.NONTERM_DEFINE_VAR || x.Term.Name == DaxGrammar.NONTERM_DEFINE_MEASURE);
        }

        public ParseTreeNode GetBottomCoveringNode(ParseTreeNode parseTreeNode)
        {
            var res = parseTreeNode;
            while (true)
            {
                if(res.ChildNodes.Where(x => x.Term.Name != null).Count() == 1)
                {
                    res = res.ChildNodes.First(x => x.Term.Name != null);
                    continue;
                }

                if (res.ChildNodes.Count == 3 && res.ChildNodes[0].Term.Name == "(" && res.ChildNodes[2].Term.Name == ")")
                {
                    res = res.ChildNodes[1];
                    continue;
                }

                break;
            }
            return res;
        }

        public List<ParseTreeNode> ListOperationArguments(ParseTreeNode operation)
        {
            var parameters = DFTraverseInner(operation).Where(x => x.Term.Name == DaxGrammar.NONTERM_PARAMETERS).FirstOrDefault();
            if (parameters == null)
            {
                return new List<ParseTreeNode>();
            }
            var arguments = parameters.ChildNodes.Where(x => x.Term.Name == DaxGrammar.NONTERM_EXPRESSION).OrderBy(x => x.Span.EndPosition).ToList();   
            return arguments;
        }

        public LocalDefinitionType GetLocalDefinitionType(ParseTreeNode parseTreeNode)
        {
            switch (parseTreeNode.Term.Name)
            {
                case DaxGrammar.NONTERM_DEFINE_VAR:
                    return LocalDefinitionType.Variable;
                case DaxGrammar.NONTERM_DEFINE_MEASURE:
                    return LocalDefinitionType.Measure;
                default:
                    throw new Exception("Could not resolve local definition type " + parseTreeNode.Term.Name);
            }
        }

        public string GetDefinition(ParseTreeNode node)
        {
            return node.GetText(_tree.SourceText);
        }

        public string GetFirstIdContent(ParseTreeNode root)
        {
            return DFTraverseInner(root).Where(x => new string[] {
                DaxGrammar.NONTERM_ID,
                DaxGrammar.NONTERM_ID_UNQUOTED,
                DaxGrammar.NONTERM_TABLE_ID,
                DaxGrammar.NONTERM_TABLE_ID_QUOTED,
                DaxGrammar.NONTERM_COLUMN_ID,
                DaxGrammar.NONTERM_COLUMN_ID_QUOTED,
                DaxGrammar.NONTERM_FULL_ID
            }.Contains(x.Term.Name)).First().GetText(_tree.SourceText);
        }

        public ParseTreeNode GetFirstId(ParseTreeNode root)
        {
            return DFTraverseInner(root).Where(x => x.Term.Name == DaxGrammar.NONTERM_ID).First();
        }

        public ParseTreeNode GetTopEvaluate()
        {
            return DFTraverse(_tree).FirstOrDefault(x => x != null && x.Term.Name == DaxGrammar.NONTERM_EVALUATE);
        }

        public DaxScriptType FindScriptType(out ParseTreeNode rootNode)
        {
            var distItem = DFTraverse(_tree).FirstOrDefault(x => x != null && new string[] { DaxGrammar.NONTERM_FORMULA, DaxGrammar.NONTERM_EXPRESSION, DaxGrammar.NONTERM_QUERY }.Contains(x.Term.Name));
            switch (distItem.Term.Name)
            {
                case DaxGrammar.NONTERM_EXPRESSION:
                    rootNode = distItem;
                    return DaxScriptType.Expression;
                case DaxGrammar.NONTERM_QUERY:
                    rootNode = distItem;
                    return DaxScriptType.Query;
                case DaxGrammar.NONTERM_FORMULA:
                    var innerExpressionRoot = DFTraverseInner(distItem).First(x => x != null && x.Term.Name == DaxGrammar.NONTERM_EXPRESSION);
                    rootNode = innerExpressionRoot;
                    return DaxScriptType.Expression;
                default:
                    throw new Exception("Could not find root of query: " + _tree.SourceText);
            }
        }
    }
}
