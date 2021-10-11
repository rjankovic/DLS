using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace CD.DLS.Parse.Mssql.PowerQuery
{
    public class Assignment
    { 
        public ParseTreeNode AssignmentNode { get; set; }
        public string Name { get; set; }
        public ParseTreeNode ExpressionNode { get; set; }
    }

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
            return DFTraverseInner(root).FirstOrDefault(x => x.Term.Name == MGrammar.NONTERM_EXPRESSION || x.Term.Name == MGrammar.NONTERM_PRIMARY_EXPRESSION); // FindTermByName(root, MGrammar.NONTERM_EXPRESSION);
        }

        public IEnumerable<Assignment> GetRecordItems(ParseTreeNode root)
        {
            return DFTraverseInner(root).First(x => x.Term.Name == MGrammar.NONTERM_RECORD_ITEMS)
                .ChildNodes.Where(x => x.Term.Name == MGrammar.NONTERM_ASSIGNMENT)
                .Select(x => new Assignment() { Name = FindTermByName(x, MGrammar.TERM_ID).GetText(Script), ExpressionNode = x.ChildNodes.Last(), AssignmentNode = x });
        }

        public List<Assignment> GetQuerySteps()
        {
            var res = DFTraverse(_tree).Where(x => x.Term.Name == MGrammar.NONTERM_QUERY_STEPS).First()
                .ChildNodes.Where(x => x.Term.Name == MGrammar.NONTERM_ASSIGNMENT)
                .Select(x => new Assignment() { Name = FindTermByName(x, MGrammar.TERM_ID).GetText(Script), ExpressionNode = x.ChildNodes.Last(), AssignmentNode = x }).ToList();

            return res;
        }

        public ParseTreeNode GetQueryOutputExpression()
        {
            return _tree.Root.ChildNodes.Last(x => x.Term.Name == MGrammar.NONTERM_EXPRESSION);
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

                if (res.ChildNodes.Count == 3 && res.ChildNodes[0].Term.Name == "{" && res.ChildNodes[2].Term.Name == "}")
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
            var parameters = FindTermByName(operation, MGrammar.NONTERM_PARAMETERS);
            if (parameters == null)
            {
                return new List<ParseTreeNode>();
            }
            var arguments = parameters.ChildNodes.Where(x => x.Term.Name == MGrammar.NONTERM_PARAMETER).OrderBy(x => x.Span.EndPosition).ToList();   
            return arguments;
        }


        public string GetDefinition(ParseTreeNode node)
        {
            return node.GetText(_tree.SourceText);
        }

        public List<ParseTreeNode> GetIndices(ParseTreeNode root)
        {
            ParseTreeNode indices = null;
            if (root.Term.Name == MGrammar.NONTERM_INDICES)
            {
                indices = root;
            }
            else
            {
                indices = DFTraverseInner(root).First(x => x.Term.Name == MGrammar.NONTERM_INDICES);
            }

            return indices.ChildNodes.ToList();
        }

        public string GetFirstIdContent(ParseTreeNode root)
        {
            return DFTraverseInner(root).Where(x => new string[] {
                MGrammar.TERM_ID
            }.Contains(x.Term.Name)).First().GetText(_tree.SourceText);
        }

        public ParseTreeNode GetFirstId(ParseTreeNode root)
        {
            return DFTraverseInner(root).Where(x => x.Term.Name == MGrammar.TERM_ID).First();
        }

    }
}
