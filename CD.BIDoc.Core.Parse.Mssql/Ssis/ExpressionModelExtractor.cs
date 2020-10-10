using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Ssis;
using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Ssis
{
    /// <summary>
    /// Extracts models for SSIS expressions.
    /// </summary>
    public class ExpressionModelExtractor
    {
        private Parser _parser;
        private ExpressionGrammar _grammar;
        private UrnBuilder _urnBuilder;

        /// <summary>
        /// Creates a new SSIS expression model extractor.
        /// </summary>
        /// <param name="urnBuilder">Urn builder used to build ref paths for model elements.</param>
        public ExpressionModelExtractor(UrnBuilder urnBuilder)
        {
            _grammar = new ExpressionGrammar();
            _parser = new Parser(_grammar);
            _urnBuilder = urnBuilder;
        }

        /// <summary>
        /// Parses the SSIS expression and creates model elements for it.
        /// </summary>
        /// <param name="expression">SSIS expression string.</param>
        /// <param name="referrables">Index of referrables that can be referenced from the expression.</param>
        /// <param name="parent">Parent element of the expression model.</param>
        /// <returns>Root element of the expression model.</returns>
        public SsisModelElement ExtractExpressionModel(string expression, SsisIndex referrables, SsisModelElement parent, string expressionName = null)
        {
            if (expression == null)
                throw new ArgumentNullException();

            // Create the root element
            RefPath expressionUrn = _urnBuilder.GetExpressionUrn(parent.RefPath, expressionName);
            var rootElement = new SsisExpressionFragmentElement(expressionUrn, "Expression", expression, parent);

            // Parse and traverse the parse tree
            ParseTree expressionTree = _parser.Parse(expression);
            if (expressionTree.Root == null)
            {
                ConfigManager.Log.Error(string.Format("Error parsing SSIS expression {0} in {1}: {2}", expression, rootElement.RefPath.Path, expressionTree.ParserMessages.FirstOrDefault()));
                return rootElement;
            }
            rootElement.OffsetFrom = expressionTree.Root.Span.EndPosition - expressionTree.Root.Span.Length;
            rootElement.Length = expressionTree.Root.Span.Length;
            BuildModelFromParseTree(expressionTree, rootElement, expression, referrables);

            return rootElement;
        }

        private string GetVariableName(ParseTreeNode variableNode, string expression)
        {
            if (variableNode.ChildNodes.Count != 2)
                throw new Exception("Unexpected structure of SSIS parse tree");

            ParseTreeNode nameNode = variableNode.ChildNodes[1];
            return nameNode.GetText(expression);
        }


        private void BuildModelFromParseTree(ParseTree expressionTree, SsisExpressionFragmentElement rootElement, string expression, SsisIndex referrables)
        {
            // Collect reference parse tree nodes
            List<ParseTreeNode> referenceNodes = new List<ParseTreeNode>();
            CollectReferences(expressionTree.Root, referenceNodes);

            // Create fragment nodes
            int fragmentCounter = 0;
            foreach (ParseTreeNode reference in referenceNodes)
            {
                ++fragmentCounter;
                var fragmentUrn = _urnBuilder.GetExpressionFragmentUrn(fragmentCounter, rootElement.RefPath);

                // Location of the fragment
                int offset = reference.Span.Location.Position;
                int length = reference.Span.Length;

                // Definitions
                string definition = expression.Substring(offset, length);

                try
                {
                    if (definition.StartsWith("@"))
                    {

                        ReferrableValueElement referrable;
                        referrables.TryGetNodeByName(definition, out referrable);
                        // Create the fragment node
                        var fragment = new SsisExpressionFragmentElement(fragmentUrn, definition, definition, rootElement);
                        fragment.OffsetFrom = offset;
                        fragment.Length = length;
                        fragment.Reference = referrable;

                        rootElement.AddChild(fragment);
                    }
                    else if (definition.StartsWith("#"))
                    {

                        DfColumnElement dfColumn;

                        String lineageIdstr = definition.Substring(1, definition.Length - 1);
                        int lineageId = Int32.Parse(lineageIdstr);

                        referrables.TryGetNodeByColumnLineageId(lineageId, out dfColumn);
                        //if (dfColumn != null)
                        //{
                        //    ConfigManager.Log.Info(string.Format("{0} in {1} refers to {2}", definition, expression, dfColumn.RefPath.Path));
                        //}
                        // Create the fragment node
                        var fragment = new SsisExpressionFragmentElement(fragmentUrn, definition, definition, rootElement);
                        fragment.OffsetFrom = offset;
                        fragment.Length = length;
                        fragment.Reference = dfColumn;

                        rootElement.AddChild(fragment);
                    }
                }
                catch
                {
                    ConfigManager.Log.Error(string.Format("Error parsing SSIS fragment {0} in {1}", definition, expression));
                    throw;
                }

            }
        }

        /// <summary>
        /// Collects parse tree nodes that are references.
        /// </summary>
        /// <param name="node">Root of a parse subtree.</param>
        /// <param name="referenceNodes">List where the reference nodes will be stored.</param>
        private void CollectReferences(ParseTreeNode node, List<ParseTreeNode> referenceNodes)
        {
            // Check if the root should be added
            if (node.Term == _grammar.VariableTerm || node.Term == _grammar.ColumnTerm)
            {
                referenceNodes.Add(node);
            }
            // Traverse all subtrees
            foreach (ParseTreeNode childNode in node.ChildNodes)
            {
                CollectReferences(childNode, referenceNodes);
            }
        }

    }
}
