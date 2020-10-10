using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace CD.DLS.Parse.Mssql
{
    /// <summary>
    /// Operates on a parse tree created by Irony, searches for terms (subtrees) of a given type
    /// </summary>
    public class ParseTreeNavigator
    {
        protected ParseTree _tree;

        public ParseTreeNavigator(ParseTree tree = null)
        {
            _tree = tree;
        }

        public static IEnumerable<ParseTreeNode> DFTraverse(ParseTree parseTree)
        {
            return DFTraverseInner(parseTree.Root);
        }

        public IEnumerable<ParseTreeNode> DFTraverse()
        {
            if (_tree == null)
            {
                return new List<ParseTreeNode>();
            }
            return DFTraverseInner(_tree.Root);
        }

        public static IEnumerable<ParseTreeNode> DFTraverseInner(ParseTreeNode node)
        {
            yield return node;
            foreach (var child in node.ChildNodes)
            {
                foreach (var childTraverseItem in DFTraverseInner(child))
                {
                    yield return childTraverseItem;
                }
            }
        }

        public List<Tuple<string, string>> GetTermsAndContent(ParseTreeNode node, string sourceText)
        {
            return DFTraverseInner(node).Select(x => new Tuple<string, string>(x.Term.Name, x.GetText(sourceText))).ToList();
        }
    }

    public static class ParseTreeNodeExtensions
    {
        public static List<string> GetTokens(this ParseTreeNode node)
        {
            return ParseTreeNavigator.DFTraverseInner(node).Where(x => x.Token != null).Select(x => x.Token.ValueString).ToList();
        }

        public static string GetText(this ParseTreeNode node, string sourceText)
        {
            return sourceText.Substring(node.Span.EndPosition - node.Span.Length, node.Span.Length);
        }
    }
}
