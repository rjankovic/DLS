using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.BIDoc.Core.Parse.Mssql.PowerQuery
{
    [Language("M", "1", "Power Query")]
    public class MGrammar : Grammar
    {
        private IdentifierTerminal CreateIdentifier()
        {
            var id = new IdentifierTerminal("id", "@$%^*_'.?-", "@$_");
            StringLiteral term = new StringLiteral("id_qouted");
            term.AddStartEnd("#\"", "\"", StringOptions.NoEscapes);
            term.SetOutputTerminal(this, id); //term will be added to NonGrammarTerminals automatically 
            return id;
        }

        public MGrammar()
            : base(false)
        {
            var IN = ToTerm("in");
            var LET = ToTerm("let");
            var COMMA = ToTerm(",");
            var LPAREN = ToTerm("(");
            var RPAREN = ToTerm(")");
            var LSET = ToTerm("{");
            var RSET = ToTerm("}");
            var LSQBR = ToTerm("[");
            var RSQBR = ToTerm("]");
            var EQ = ToTerm("=");
            var NE = ToTerm("<>");
            var LT = ToTerm("<");
            var LE = ToTerm("<=");
            var GT = ToTerm(">");
            var GE = ToTerm(">=");

            var EXPONENT = ToTerm("^");

            var DIVIDE = ToTerm("/");
            var MULTIPLY = ToTerm("*");

            var AND = ToTerm("and");
            var CONCAT = ToTerm("&");
            var OR = ToTerm("or");

            var NOT = ToTerm("not");
            var EACH = ToTerm("each");
            var EXCL = ToTerm("!");

            var MINUS = ToTerm("-");
            var TYPE = ToTerm("type");
            var AS = ToTerm("as");
            var PLUS = ToTerm("+");

            var RARROW = ToTerm("=>");

            var IF = ToTerm("if");
            var THEN = ToTerm("then");
            var ELSE = ToTerm("else");

            CommentTerminal SingleLineComment = new CommentTerminal("SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            CommentTerminal DelimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            NonGrammarTerminals.Add(SingleLineComment);
            NonGrammarTerminals.Add(DelimitedComment);

            var number = new NumberLiteral("number");
            number.AddPrefix("0x", NumberOptions.Hex);
            var stringLiteral = new StringLiteral("string", "\"", StringOptions.AllowsDoubledQuote);

            var identifier = CreateIdentifier();

            var expression = new NonTerminal("expression");

            var functionSpec = new NonTerminal("functionSpec");

            var parameter = new NonTerminal("parameter");
            parameter.Rule = expression | expression + AS + identifier | functionSpec;

            var parameters = new NonTerminal("parameters");
            parameters.Rule = MakePlusRule(parameters, COMMA, parameter);

            var functionCall = new NonTerminal("functionCall");
            functionCall.Rule = identifier + (LPAREN + parameters + RPAREN | LPAREN + RPAREN);


            var assignment = new NonTerminal("assignment");
            assignment.Rule = identifier + EQ + expression | identifier + EQ + functionSpec;

            var querySteps = new NonTerminal("query_steps");
            querySteps.Rule = MakePlusRule(querySteps, COMMA, assignment);

            var recordItems = new NonTerminal("record_items");
            recordItems.Rule = MakePlusRule(recordItems, COMMA, assignment);

            var query = new NonTerminal("query");
            query.Rule = LET + querySteps + IN + expression;

            var functionBody = new NonTerminal("functionBody");
            functionBody.Rule = query | expression;

            functionSpec.Rule = LPAREN + parameters + RPAREN + RARROW + functionBody | LPAREN + parameters + RPAREN + AS + identifier + RARROW + functionBody;


            var index = new NonTerminal("index");
            index.Rule = LSET + expression + RSET;

            var indices = new NonTerminal("indices");
            indices.Rule = MakePlusRule(indices, index);

            var listExpression = new NonTerminal("listExpression");
            listExpression.Rule = LSET + parameters + RSET + indices | LSET + RSET + indices | LSET + RSET | LSET + parameters + RSET;

            var listAccessExpression = new NonTerminal("listAccessExpression");
            listAccessExpression.Rule = identifier + indices;

            var recordExpression = new NonTerminal("recordExpression");
            recordExpression.Rule = LSQBR + recordItems + RSQBR | LSQBR + RSQBR;

            var primaryExpression = new NonTerminal("primaryExpression");

            var unaryArithmenticOperator = new NonTerminal("unaryArithmeticOperator");
            unaryArithmenticOperator.Rule = MINUS | PLUS | TYPE; //NOT | MINUS | EXCL;

            var columnId = new NonTerminal("columnId");
            columnId.Rule = LSQBR + identifier + RSQBR;

            var unaryArithmeticExpression = new NonTerminal("unaryArithmenticExpression");
            unaryArithmeticExpression.Rule = unaryArithmenticOperator + unaryArithmeticExpression | primaryExpression | primaryExpression + columnId;

            var mulExpression = MakeInfixOperator("mul", MULTIPLY | DIVIDE, unaryArithmeticExpression);
            var addExpression = MakeInfixOperator("add", PLUS | MINUS, mulExpression);
            var concatExpression = MakeInfixOperator("concat", CONCAT, addExpression);

            var relExpression = MakeInfixOperator("rel", LT | LE | GT | GE, concatExpression);
            var eqExpression = MakeInfixOperator("eq", EQ | NE, relExpression);

            var boolUnaryExpression = new NonTerminal("boolUnaryExpression");
            boolUnaryExpression.Rule = NOT + eqExpression | eqExpression;
            var boolAndExpression = MakeInfixOperator("boolAnd", AND, boolUnaryExpression);
            var boolOrExpression = MakeInfixOperator("boolOr", OR, boolAndExpression);

            var conditionalExpression = new NonTerminal("conditionalExpression");
            conditionalExpression.Rule = IF + expression + THEN + expression + ELSE + expression;

            expression.Rule = boolOrExpression | EACH + boolOrExpression | conditionalExpression;

            primaryExpression.Rule = functionCall | number | stringLiteral | recordExpression | listExpression | listAccessExpression | identifier | columnId;


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
