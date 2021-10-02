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
    [Language("DAX", "1", "DAX")]
    public class DaxGrammar : Grammar
    {
        private IdentifierTerminal CreateDaxColumnIdentifier()
        {
            var id = new IdentifierTerminal("column_id");
            StringLiteral term = new StringLiteral("column_id_qouted");
            term.AddStartEnd("[", "]", StringOptions.NoEscapes);
            term.SetOutputTerminal(this, id); //term will be added to NonGrammarTerminals automatically 
            return id;
        }

        private IdentifierTerminal CreateDaxTableIdentifier()
        {
            var id = new IdentifierTerminal("table_id");
            StringLiteral term = new StringLiteral("table_id_qouted");
            term.AddStartEnd("'", "'", StringOptions.NoEscapes);
            term.SetOutputTerminal(this, id); //term will be added to NonGrammarTerminals automatically 
            return id;
        }

        private IdentifierTerminal CreateTerm(string name)
        {
            IdentifierTerminal term = new IdentifierTerminal(name, "!@#$%^*_'.?-", "!@#$%^*_'.?0123456789");
            term.CharCategories.AddRange(new UnicodeCategory[] {
             UnicodeCategory.UppercaseLetter, //Ul
             UnicodeCategory.LowercaseLetter, //Ll
             UnicodeCategory.TitlecaseLetter, //Lt
             UnicodeCategory.ModifierLetter,  //Lm
             UnicodeCategory.OtherLetter,     //Lo
             UnicodeCategory.LetterNumber,     //Nl
             UnicodeCategory.DecimalDigitNumber, //Nd
             UnicodeCategory.ConnectorPunctuation, //Pc
             UnicodeCategory.SpacingCombiningMark, //Mc
             UnicodeCategory.NonSpacingMark,       //Mn
             UnicodeCategory.Format                //Cf
          });
            //StartCharCategories are the same
            term.StartCharCategories.AddRange(term.CharCategories);
            //term.SetOutputTerminal(this, term); //term will be added to NonGrammarTerminals automatically
            return term;
        }

        
        public DaxGrammar()
            : base(false)
        {
            var EVALUATE = ToTerm("EVALUATE");
            var MEASURE = ToTerm("MEASURE");
            var VAR = ToTerm("VAR");
            var RETURN = ToTerm("RETURN");
            var DEFINE = ToTerm("DEFINE");
            var ASC = ToTerm("ASC");
            var DESC = ToTerm("DESC");
            var ORDER_BY = ToTerm("ORDER BY");
            var START_AT = ToTerm("START AT");
            var COMMA = ToTerm(",");
            var LPAREN = ToTerm("(");
            var RPAREN = ToTerm(")");
            var LSET = ToTerm("{");
            var RSET = ToTerm("}");
            
            var EXPONENT = ToTerm("^");

            var DIVIDE = ToTerm("/");
            var MULTIPLY = ToTerm("*");

            var NOT = ToTerm("NOT");
            var EXCL = ToTerm("!");

            var MINUS = ToTerm("-");
            var PLUS = ToTerm("+");

            var CONCAT = ToTerm("&");

            var EQ = ToTerm("=");
            var NE = ToTerm("<>");
            var LT = ToTerm("<");
            var LE = ToTerm("<=");
            var GT = ToTerm(">");
            var GE = ToTerm(">=");

            var IN = ToTerm("IN");

            var AND = ToTerm("&&");
            var OR = ToTerm("||");


            //var identifier = CreateDaxColumnIdentifier();

            // identifiers
            var id_unquoted = new IdentifierTerminal("id_unquoted");
            id_unquoted.Priority = TerminalPriority.High;
            var id = new NonTerminal("id");
            var columnId = CreateDaxColumnIdentifier();
            var tableId = CreateDaxTableIdentifier();
            var fullId = new NonTerminal("full_id");
            fullId.Rule = tableId + columnId | id_unquoted + columnId;
            // main identifier
            id.Rule = id_unquoted | columnId | tableId | fullId;

            // Literals
            var number = new NumberLiteral("number");
            number.AddPrefix("0x", NumberOptions.Hex);
            var stringLiteral = new StringLiteral("string", "\"", StringOptions.AllowsDoubledQuote);


            var expression = new NonTerminal("expression");
            expression.Rule = id;

            var formula = new NonTerminal("formula");
            formula.Rule = EQ + expression;

            var namedFormula = new NonTerminal("named_formula");
            namedFormula.Rule = id + formula;


            var defineMeasure = new NonTerminal("define_measure");
            defineMeasure.Rule = MEASURE + namedFormula;

            var defineVar = new NonTerminal("define_var");
            defineVar.Rule = VAR + namedFormula;

            var defineReturn = new NonTerminal("define_return");
            defineReturn.Rule = RETURN + expression;

            
            var defineItem = new NonTerminal("define_item");
            defineItem.Rule = defineMeasure | defineVar | defineReturn;

            var defineList = new NonTerminal("define_list");
            defineList.Rule = MakePlusRule(defineList, Empty, defineItem);

            var defineClause = new NonTerminal("define_clause");
            defineClause.Rule = DEFINE + defineList;
            

            var orderItem = new NonTerminal("order_item");
            orderItem.Rule = expression | expression + ASC | expression + DESC;

            var orderList = new NonTerminal("order_list");
            orderList.Rule = MakePlusRule(orderList, COMMA, orderItem);

            var startAtItem = new NonTerminal("start_at_item");
            startAtItem.Rule = expression;

            var startAtList = new NonTerminal("start_at_list");
            startAtList.Rule = MakePlusRule(startAtList, COMMA, startAtItem);

            var orderClause = new NonTerminal("order_clause");
            orderClause.Rule =
                ORDER_BY + orderList
                | ORDER_BY + orderList + START_AT + startAtList;

            var evaluate = new NonTerminal("evaluate");
            evaluate.Rule = EVALUATE + expression | EVALUATE + expression + orderClause;
            
            var evaluateList = new NonTerminal("evaluate_list");
            evaluateList.Rule = MakePlusRule(evaluateList, Empty, evaluate);

            var query = new NonTerminal("query");
            query.Rule = evaluateList | defineClause + evaluateList;
            
            var daxRoot = new NonTerminal("dax_root");
            daxRoot.Rule = expression | formula /*| namedFormula*/ | query;
            
            var parameters = new NonTerminal("parameters");
            parameters.Rule = MakePlusRule(parameters, COMMA, expression);

            var setExpression = new NonTerminal("setExpression");
            setExpression.Rule = LSET + parameters + RSET | LSET + RSET;

            var inExpression = new NonTerminal("inOperator");
            inExpression.Rule = expression + IN + setExpression;

            var functionCall = new NonTerminal("functionCall");
            functionCall.Rule = id_unquoted + (LPAREN + parameters + RPAREN | LPAREN + RPAREN);

            var tuple = new NonTerminal("tuple");
            tuple.Rule = LPAREN + expression + COMMA + parameters + RPAREN;
            
            var primaryExpression = new NonTerminal("primaryExpression");
            primaryExpression.Rule = functionCall | number | stringLiteral | id 
                | LPAREN + expression + RPAREN | setExpression | tuple;
            

            var unaryArithmenticOperator = new NonTerminal("unaryArithmeticOperator");
            unaryArithmenticOperator.Rule = MINUS; //NOT | MINUS | EXCL;

            var unaryArithmeticExpression = new NonTerminal("unaryArithmenticExpression");
            unaryArithmeticExpression.Rule = unaryArithmenticOperator + unaryArithmeticExpression | primaryExpression;

            var mulExpression = MakeInfixOperator("mul", MULTIPLY | DIVIDE, unaryArithmeticExpression);
            var addExpression = MakeInfixOperator("add", PLUS | MINUS, mulExpression);
            var concatExpression = MakeInfixOperator("concat", CONCAT, addExpression);

            var relExpression = MakeInfixOperator("rel", LT | LE | GT | GE, concatExpression);
            var eqExpression = MakeInfixOperator("eq", EQ | NE, relExpression);

            var boolUnaryExpression = new NonTerminal("boolUnaryExpression");
            boolUnaryExpression.Rule = NOT + eqExpression | eqExpression;
            var boolAndExpression = MakeInfixOperator("boolAnd", AND, boolUnaryExpression);
            var boolOrExpression = MakeInfixOperator("boolOr", OR, boolAndExpression);

            expression.Rule = boolOrExpression | inExpression;


            Root = daxRoot;
            
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
