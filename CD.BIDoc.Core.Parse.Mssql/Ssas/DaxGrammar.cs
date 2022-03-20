using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace CD.DLS.Parse.Mssql.Ssas
{
    /// <summary>
    /// DAX grammar based on https://docs.microsoft.com/en-us/dax/dax-syntax-reference and https://docs.microsoft.com/en-us/dax/dax-queries
    /// </summary>
    [Language("DAX", "1", "DAX grammar")]
    public class DaxGrammar : Grammar
    {
        public const string NONTERM_ID_UNQUOTED = "id_unquoted";
        public const string NONTERM_ID = "id";
        public const string NONTERM_FULL_ID = "full_id";
        public const string NONTERM_NUMBER = "number";
        public const string NONTERM_STRING = "string";
        public const string NONTERM_EXPRESSION = "expression";
        public const string NONTERM_FORMULA = "formula";
        public const string NONTERM_NAMED_FORMULA = "named_formula";
        public const string NONTERM_DEFINE_MEASURE = "define_measure";
        public const string NONTERM_DEFINE_VAR = "define_var";
        public const string NONTERM_DEFINE_RETURN = "define_return";
        public const string NONTERM_DEFINE_ITEM = "define_item";
        public const string NONTERM_DEFINE_LIST = "define_list";
        public const string NONTERM_DEFINE_CLAUSE = "define_clause";
        public const string NONTERM_ORDER_ITEM = "order_item";
        public const string NONTERM_ORDER_LIST = "order_list";
        public const string NONTERM_START_AT_ITEM = "start_at_item";
        public const string NONTERM_START_AT_LIST = "start_at_list";
        public const string NONTERM_ORDER_CLAUSE = "order_clause";
        public const string NONTERM_EVALUATE = "evaluate";
        public const string NONTERM_EVALUATE_LIST = "evaluate_list";
        public const string NONTERM_QUERY = "query";
        public const string NONTERM_DAX_ROOT = "dax_root";
        public const string NONTERM_PARAMETERS = "parameters";
        public const string NONTERM_SET_EXPRESSION = "setExpression";
        public const string NONTERM_IN_OPERATOR = "inOperator";
        public const string NONTERM_FUNCTION_CALL = "functionCall";
        public const string NONTERM_TUPLE = "tuple";
        public const string NONTERM_PRIMARY_EXPRESSION = "primaryExpression";
        public const string NONTERM_UNARY_ARITHMETIC_OPERATOR = "unaryArithmeticOperator";
        public const string NONTERM_UNARY_ARITHMETIC_EXPRESSION = "unaryArithmenticExpression";

        public const string NONTERM_MUL = "mul";
        public const string NONTERM_ADD = "add";
        public const string NONTERM_CONCAT = "concat";
        public const string NONTERM_REL = "rel";
        public const string NONTERM_EQ = "eq";
        public const string NONTERM_BOOL_UNARY = "boolUnaryExpression";
        public const string NONTERM_BOOL_AND = "boolAnd";
        public const string NONTERM_BOOL_OR = "boolOr";

        public const string NONTERM_COLUMN_ID = "column_id";
        public const string NONTERM_COLUMN_ID_QUOTED = "column_id_qouted";
        public const string NONTERM_TABLE_ID = "table_id";
        public const string NONTERM_TABLE_ID_QUOTED = "table_id_qouted";

        private IdentifierTerminal CreateDaxColumnIdentifier()
        {
            var id = new IdentifierTerminal(NONTERM_COLUMN_ID);
            StringLiteral term = new StringLiteral(NONTERM_COLUMN_ID_QUOTED);
            term.AddStartEnd("[", "]", StringOptions.NoEscapes);
            term.SetOutputTerminal(this, id); //term will be added to NonGrammarTerminals automatically 
            return id;
        }

        private IdentifierTerminal CreateDaxTableIdentifier()
        {
            var id = new IdentifierTerminal(NONTERM_TABLE_ID);
            StringLiteral term = new StringLiteral(NONTERM_TABLE_ID_QUOTED);
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
            var id = new NonTerminal(NONTERM_ID);
            var columnId = CreateDaxColumnIdentifier();
            var tableId = CreateDaxTableIdentifier();
            var fullId = new NonTerminal(NONTERM_FULL_ID);
            fullId.Rule = tableId + columnId | id_unquoted + columnId;
            // main identifier
            id.Rule = id_unquoted | columnId | tableId | fullId;

            // Literals
            var number = new NumberLiteral(NONTERM_NUMBER);
            number.AddPrefix("0x", NumberOptions.Hex);
            var stringLiteral = new StringLiteral(NONTERM_STRING, "\"", StringOptions.NoEscapes | StringOptions.AllowsDoubledQuote);


            var expression = new NonTerminal(NONTERM_EXPRESSION);
            expression.Rule = id;

            var formula = new NonTerminal(NONTERM_FORMULA);
            formula.Rule = EQ + expression;

            var namedFormula = new NonTerminal(NONTERM_NAMED_FORMULA);
            namedFormula.Rule = id + formula;


            var defineMeasure = new NonTerminal(NONTERM_DEFINE_MEASURE);
            defineMeasure.Rule = MEASURE + namedFormula;

            var defineVar = new NonTerminal(NONTERM_DEFINE_VAR);
            defineVar.Rule = VAR + namedFormula;

            var defineReturn = new NonTerminal(NONTERM_DEFINE_RETURN);
            defineReturn.Rule = RETURN + expression;


            var defineItem = new NonTerminal(NONTERM_DEFINE_ITEM);
            defineItem.Rule = defineMeasure | defineVar | defineReturn;

            var defineList = new NonTerminal(NONTERM_DEFINE_LIST);
            defineList.Rule = MakePlusRule(defineList, Empty, defineItem);

            var defineClause = new NonTerminal(NONTERM_DEFINE_CLAUSE);
            defineClause.Rule = DEFINE + defineList;


            var orderItem = new NonTerminal(NONTERM_ORDER_ITEM);
            orderItem.Rule = expression | expression + ASC | expression + DESC;

            var orderList = new NonTerminal(NONTERM_ORDER_LIST);
            orderList.Rule = MakePlusRule(orderList, COMMA, orderItem);

            var startAtItem = new NonTerminal(NONTERM_START_AT_ITEM);
            startAtItem.Rule = expression;

            var startAtList = new NonTerminal(NONTERM_START_AT_LIST);
            startAtList.Rule = MakePlusRule(startAtList, COMMA, startAtItem);

            var orderClause = new NonTerminal(NONTERM_ORDER_CLAUSE);
            orderClause.Rule =
                ORDER_BY + orderList
                | ORDER_BY + orderList + START_AT + startAtList;

            var evaluate = new NonTerminal(NONTERM_EVALUATE);
            evaluate.Rule = EVALUATE + expression | EVALUATE + expression + orderClause;

            var evaluateList = new NonTerminal(NONTERM_EVALUATE_LIST);
            evaluateList.Rule = MakePlusRule(evaluateList, Empty, evaluate);

            var query = new NonTerminal(NONTERM_QUERY);
            query.Rule = evaluateList | defineClause + evaluateList | defineList + defineReturn; ;

            var daxRoot = new NonTerminal(NONTERM_DAX_ROOT);
            daxRoot.Rule = expression | formula /*| namedFormula*/ | query;

            var parameters = new NonTerminal(NONTERM_PARAMETERS);
            parameters.Rule = MakePlusRule(parameters, COMMA, expression);

            var setExpression = new NonTerminal(NONTERM_SET_EXPRESSION);
            setExpression.Rule = LSET + parameters + RSET | LSET + RSET;

            var inExpression = new NonTerminal(NONTERM_IN_OPERATOR);
            inExpression.Rule = expression + IN + setExpression;

            var functionCall = new NonTerminal(NONTERM_FUNCTION_CALL);
            functionCall.Rule = id_unquoted + (LPAREN + parameters + RPAREN | LPAREN + RPAREN);

            var tuple = new NonTerminal(NONTERM_TUPLE);
            tuple.Rule = LPAREN + expression + COMMA + parameters + RPAREN;

            var primaryExpression = new NonTerminal(NONTERM_PRIMARY_EXPRESSION);
            primaryExpression.Rule = functionCall | number | stringLiteral | id
                | LPAREN + expression + RPAREN | setExpression | tuple;


            var unaryArithmenticOperator = new NonTerminal(NONTERM_UNARY_ARITHMETIC_OPERATOR);
            unaryArithmenticOperator.Rule = MINUS; //NOT | MINUS | EXCL;

            var unaryArithmeticExpression = new NonTerminal(NONTERM_UNARY_ARITHMETIC_EXPRESSION);
            unaryArithmeticExpression.Rule = unaryArithmenticOperator + unaryArithmeticExpression | primaryExpression;

            var mulExpression = MakeInfixOperator(NONTERM_MUL, MULTIPLY | DIVIDE, unaryArithmeticExpression);
            var addExpression = MakeInfixOperator(NONTERM_ADD, PLUS | MINUS, mulExpression);
            var concatExpression = MakeInfixOperator(NONTERM_CONCAT, CONCAT, addExpression);

            var relExpression = MakeInfixOperator(NONTERM_REL, LT | LE | GT | GE, concatExpression);
            var eqExpression = MakeInfixOperator(NONTERM_EQ, EQ | NE, relExpression);

            var boolUnaryExpression = new NonTerminal(NONTERM_BOOL_UNARY);
            boolUnaryExpression.Rule = NOT + eqExpression | eqExpression;
            var boolAndExpression = MakeInfixOperator(NONTERM_BOOL_AND, AND, boolUnaryExpression);
            var boolOrExpression = MakeInfixOperator(NONTERM_BOOL_OR, OR, boolAndExpression);

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
            var expressionNonTerminal = new NonTerminal(name/* + "Expression"*/);
            expressionNonTerminal.Rule = expressionNonTerminal + operatorNonTerminal + innerNonTerminal | innerNonTerminal;

            return expressionNonTerminal;
        }
    }
}