using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.PowerQuery
{
    [Language("M", "1", "Power Query")]
    public class MGrammar : Grammar
    {
        public const string TERM_NUMBER = "number";
        public const string TERM_STRING = "string";
        public const string NONTERM_EXPRESSION = "expression";
        public const string NONTERM_FUNCTION_SPEC = "functionSpec";
        public const string NONTERM_PARAMETER = "parameter";
        public const string NONTERM_PARAMETERS = "parameters";
        public const string NONTERM_FUNCTION_CALL = "functionCall";
        public const string NONTERM_ASSIGNMENT = "assignment";
        public const string NONTERM_QUERY_STEPS = "query_steps";
        public const string NONTERM_RECORD_ITEMS = "record_items";
        public const string NONTERM_QUERY = "query";
        public const string NONTERM_FUNCTION_BODY = "functionBody";
        public const string NONTERM_INDEX = "index";
        public const string NONTERM_INDICES = "indices";
        public const string NONTERM_LIST_EXPRESSION = "listExpression";
        public const string NONTERM_LIST_ACCESS_EXPRESSION = "listAccessExpression";
        public const string NONTERM_RECORD_EXPRESSION = "recordExpression";
        public const string NONTERM_PRIMARY_EXPRESSION = "primaryExpression";
        public const string NONTERM_UNARY_OPERATOR = "unaryArithmeticOperator";
        public const string NONTERM_COLUMN_ID = "columnId";
        public const string NONTERM_UNARY_EXPRESSION = "unaryArithmenticExpression";
        public const string NONTERM_CONDITIONAL_EXPRESSION = "conditionalExpression";
        
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

            var number = new NumberLiteral(TERM_NUMBER);
            number.AddPrefix("0x", NumberOptions.Hex);
            var stringLiteral = new StringLiteral(TERM_STRING, "\"", StringOptions.AllowsDoubledQuote);

            var identifier = CreateIdentifier();

            var expression = new NonTerminal(NONTERM_EXPRESSION);

            var functionSpec = new NonTerminal(NONTERM_FUNCTION_SPEC);

            var parameter = new NonTerminal(NONTERM_PARAMETER);
            parameter.Rule = expression | expression + AS + identifier | functionSpec;

            var parameters = new NonTerminal(NONTERM_PARAMETERS);
            parameters.Rule = MakePlusRule(parameters, COMMA, parameter);

            var functionCall = new NonTerminal(NONTERM_FUNCTION_CALL);
            functionCall.Rule = identifier + (LPAREN + parameters + RPAREN | LPAREN + RPAREN);


            var assignment = new NonTerminal(NONTERM_ASSIGNMENT);
            assignment.Rule = identifier + EQ + expression | identifier + EQ + functionSpec;

            var querySteps = new NonTerminal(NONTERM_QUERY_STEPS);
            querySteps.Rule = MakePlusRule(querySteps, COMMA, assignment);

            var recordItems = new NonTerminal(NONTERM_RECORD_ITEMS);
            recordItems.Rule = MakePlusRule(recordItems, COMMA, assignment);

            var query = new NonTerminal(NONTERM_QUERY);
            query.Rule = LET + querySteps + IN + expression;

            var functionBody = new NonTerminal(NONTERM_FUNCTION_BODY);
            functionBody.Rule = query | expression;

            functionSpec.Rule = LPAREN + parameters + RPAREN + RARROW + functionBody | LPAREN + parameters + RPAREN + AS + identifier + RARROW + functionBody;


            var index = new NonTerminal(NONTERM_INDEX);
            index.Rule = LSET + expression + RSET;

            var indices = new NonTerminal(NONTERM_INDICES);
            indices.Rule = MakePlusRule(indices, index);

            var listExpression = new NonTerminal(NONTERM_LIST_EXPRESSION);
            listExpression.Rule = LSET + parameters + RSET + indices | LSET + RSET + indices | LSET + RSET | LSET + parameters + RSET;

            var listAccessExpression = new NonTerminal(NONTERM_LIST_ACCESS_EXPRESSION);
            listAccessExpression.Rule = identifier + indices;

            var recordExpression = new NonTerminal(NONTERM_RECORD_EXPRESSION);
            recordExpression.Rule = LSQBR + recordItems + RSQBR | LSQBR + RSQBR;

            var primaryExpression = new NonTerminal(NONTERM_PRIMARY_EXPRESSION);

            var unaryArithmenticOperator = new NonTerminal(NONTERM_UNARY_OPERATOR);
            unaryArithmenticOperator.Rule = MINUS | PLUS | TYPE; //NOT | MINUS | EXCL;

            var columnId = new NonTerminal(NONTERM_COLUMN_ID);
            columnId.Rule = LSQBR + identifier + RSQBR;

            var unaryArithmeticExpression = new NonTerminal(NONTERM_UNARY_EXPRESSION);
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

            var conditionalExpression = new NonTerminal(NONTERM_CONDITIONAL_EXPRESSION);
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
