using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace CD.DLS.Parse.Mssql.Ssas
{
    public class MdxParseTreeNavigator : ParseTreeNavigator
    {
        public MdxParseTreeNavigator(ParseTree tree)
            :base(tree)
        {
        }

        /// <summary>
        /// for both select scripts and cube calculations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ParseTreeNode> GetCalculatedMembers()
        {
            return DFTraverse(_tree).Where(x => x != null && x.Term.Name == "calculated_member_definition");
        }

        public IEnumerable<ParseTreeNode> GetPotentialReferences(ParseTreeNode scriptSegment)
        {
            return DFTraverseInner(scriptSegment).Where(x => x.Term.Name == "expression_property" || x.Term.Name == "property");
        }

        /// <summary>
        /// for both select scripts and cube calculations
        /// </summary>
        /// <param name="calculatedMemberDefinition"></param>
        /// <returns></returns>
        public List<string> GetCalculatedMemberNameParts(ParseTreeNode calculatedMemberDefinition)
        {
            var declaration = DFTraverseInner(calculatedMemberDefinition).First(x => x.Term.Name == "calculated_member_declaration");
            var memberName = DFTraverseInner(declaration).First(x => x.Term.Name == "member_name");
            var nameTokens = memberName.GetTokens();
            return nameTokens;
        }

        public  IEnumerable<ParseTreeNode> GetTopLevelAxisSpecifications()
        {
            var select = DFTraverse(_tree).First(x => x.Term.Name == "select_statement_subcube");
            var axes = DFTraverseInner(select).Skip(1).TakeWhile(x => x.Term.Name != "select_statement_subcube").Where(x => x.Term.Name == "axis_specification");
            return axes;
        }

        public int GetAxisId(ParseTreeNode axisSpecification, string scriptText)
        {
            var axisName = DFTraverseInner(axisSpecification).First(x => x.Term.Name == "axis_name").GetText(scriptText);
            int axisId = -1;
            if (int.TryParse(axisName, out axisId))
            {
                return axisId;
            }
            switch (axisName.ToUpper())
            {
                case "COLUMNS":
                    return 0;
                case "ROWS":
                    return 1;
                case "PAGES":
                    return 2;
                default:
                    throw new Exception(string.Format("Unrecognized axis name: {0}", axisName));
            }
            
        }

        public IEnumerable<ParseTreeNode> GetAxisItemSelection(ParseTreeNode axisSpecification)
        {
            var lowLevelList = axisSpecification;
            var expressionsList = DFTraverseInner(axisSpecification).FirstOrDefault(x => x.Term.Name == "expressions_list");
            while (expressionsList != null)
            {
                lowLevelList = expressionsList;
                expressionsList = DFTraverseInner(expressionsList).Skip(1).FirstOrDefault(x => x.Term.Name == "expressions_list");
            }
            return DFTraverseInner(lowLevelList).Where(x => x.Term.Name == "expression_property").ToList();
        }

        public IEnumerable<ParseTreeNode> GetScopeStatements()
        {
            return DFTraverse(_tree).Where(x => x.Term.Name == "scope_statement");
        }

        public ParseTreeNode GetCubeId(ParseTreeNode statement)
        {
            var cubeSpec = DFTraverseInner(statement).Last(x => x.Term.Name == "cube_specification");
            var cubeId = DFTraverseInner(cubeSpec).First(x => x.Term.Name == "id_simple");
            return cubeId;
        }
    }
}
