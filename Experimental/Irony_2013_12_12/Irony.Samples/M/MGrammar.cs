using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irony.Samples.SSIS
{
    /// <summary>
    /// Grammar for SSIS Expressions.
    /// </summary>
    /// <remarks>
    /// Variable names are case sensitive, functions and types are not.
    /// If an unqualified variable is ambiguous, there is an error.
    /// There cannot be any spaces in variable references.
    /// There cannot be any spaces between function name and left parenthersis.
    /// There can be space inside function call and cast parenthesis and around operators.
    /// Operator priority: unary;*,/,%; +,-; >=, &lt;=, >, &lt;; ==, !=;  &; ^; |;  &amp;&amp;; ||;?:
    /// 
    /// Syntax: https://msdn.microsoft.com/en-us/library/ms140206.aspx
    /// </remarks>
    [Language("M", "1", "Power Query")]
    public class MGrammar : Grammar
    {
        private IdentifierTerminal CreateIdentifier()
        {
            var id = new IdentifierTerminal("id");
            StringLiteral term = new StringLiteral("id_qouted");
            term.AddStartEnd("#\"", "\"", StringOptions.NoEscapes);
            term.SetOutputTerminal(this, id); //term will be added to NonGrammarTerminals automatically 
            return id;
        }

        public MGrammar()
            : base(false)
        {
            var IN = ToTerm("in");
            var identifier = CreateIdentifier();

            var query = new NonTerminal("query");
            query.Rule = IN + identifier;


            Root = query;

        }

        private NonTerminal MakeInfixOperator(string name, BnfExpression operators, NonTerminal innerNonTerminal)
        {
            var operatorNonTerminal = new NonTerminal(name + "Operator");
            operatorNonTerminal.Rule = operators;

            return MakeInfixOperator(name, operatorNonTerminal, innerNonTerminal);
        }
        private NonTerminal MakeInfixOperator(string name, NonTerminal operatorNonTerminal, NonTerminal innerNonTerminal)
        {
            var expressionNonTerminal = new NonTerminal(name + "Expression");
            expressionNonTerminal.Rule = expressionNonTerminal + operatorNonTerminal + innerNonTerminal | innerNonTerminal;

            return expressionNonTerminal;
        }
    }

}
