using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace CD.DLS.Parse.Mssql.Ssrs
{
    /// <summary>
    /// Grammar for SSRS Expressions.
    /// </summary>
    /// <remarks>
    /// Syntax: https://docs.microsoft.com/en-us/sql/reporting-services/report-design/expressions-report-builder-and-ssrs
    /// </remarks>
    [Language("SSRSExpression", "1", "SSRS Expressions")]
    public class ExpressionGrammar : Grammar
    {
        private IdentifierTerminal CreateSsrsIdentifier()
        {
            var id = new IdentifierTerminal("idSimple");

            id.CharCategories.AddRange(new UnicodeCategory[] {
             UnicodeCategory.UppercaseLetter, //Ul
             UnicodeCategory.LowercaseLetter, //Ll
          });

            return id;
        }

        private IdentifierTerminal CreateSsrsCompositeFuntionIdentifier()
        {
            var id = new IdentifierTerminal("compositeId", ".");
            return id;
        }

        public ExpressionGrammar()
            : base(false)
        {
            var lineComment = new CommentTerminal("line_comment", "'", "\n", "\r\n");
            NonGrammarTerminals.Add(lineComment);

            // Tokens
            var lParen = ToTerm("(");
            var rParen = ToTerm(")");
            var comma = ToTerm(",");

            var plus = ToTerm("+");
            var minus = ToTerm("-");
            var mul = ToTerm("*");
            var div = ToTerm("/");
            var mod = ToTerm("%");
            var mod2 = ToTerm("Mod");
            var and = ToTerm("&");
            var or = ToTerm("|");
            var xor = ToTerm("^");
            var bool_not = ToTerm("Not");
            var bool_orElse = ToTerm("OrElse");
            var bool_andAlso = ToTerm("AndAlso");
            var bool_xor = ToTerm("Xor");
            var bool_or = ToTerm("Or");
            var bool_and = ToTerm("And");
            var bool_is = ToTerm("Is");

            //var eqSimple = ToTerm("=");
            var eq = ToTerm("=");
            var like = ToTerm("Like");
            var ne_exclamation = ToTerm("!=");
            var ne_brackets = ToTerm("<>");
            var lt = ToTerm("<");
            var le = ToTerm("<=");
            var gt = ToTerm(">");
            var ge = ToTerm(">=");

            var exclamation = ToTerm("!");

            var question = ToTerm("?");
            var colon = ToTerm(":");
            var dot = ToTerm(".");

            // Keywords

            var nullKeyword = ToTerm("NULL");

            var idSimple = CreateSsrsIdentifier();
            var compositeId = CreateSsrsCompositeFuntionIdentifier();


            //var functionId = new NonTerminal("functionId");
            //functionId.Rule = idSimple | idSimple + dot + functionId;

            var number = new NumberLiteral("number");
            number.AddPrefix("0x", NumberOptions.Hex);
            var stringLiteral = new StringLiteral("string", "\"", StringOptions.AllowsDoubledQuote);

            // Expressions
            var expression = new NonTerminal("expression");

            var expressionRoot = new NonTerminal("expressionRoot");
            expressionRoot.Rule = eq + expression;

            Root = expressionRoot;

            // Functions
            var parameters = new NonTerminal("parameters");
            parameters.Rule = MakeListRule(parameters, comma, expression, TermListOptions.StarList); //MakePlusRule(parameters, comma, expression);
            
            //TODO: space not allowd between identifier and parenthesis
            var functionCall = new NonTerminal("functionCall");

            functionCall.Rule =
                compositeId + lParen + parameters + rParen
                | compositeId + lParen + rParen
                | idSimple + lParen + parameters + rParen
                | idSimple + lParen + rParen;

            var dataItemId = new NonTerminal("dataItemId");
            dataItemId.Rule = idSimple + exclamation + idSimple; //| idSimple + exclamation + functionCall;


            var propertyName = new NonTerminal("property");
            propertyName.Rule = idSimple | idSimple + lParen + number + rParen;

            var idSimpleOrDataItem = new NonTerminal("idSimpleOrDataItem");
            idSimpleOrDataItem.Rule = idSimple | dataItemId;

            var identifier = new NonTerminal("identifier");
            identifier.Rule =
                compositeId
                | idSimpleOrDataItem
                | (idSimpleOrDataItem + dot + propertyName)
                | (idSimpleOrDataItem + dot + propertyName + dot + functionCall);


            var primaryExpression = new NonTerminal("primaryExpression");
            primaryExpression.Rule = functionCall | number | stringLiteral | identifier | lParen + expression + rParen; //| identifier + dot + functionCall;

            var unaryOperator = new NonTerminal("unaryOperator");
            unaryOperator.Rule = minus | exclamation;

            var unaryExpression = new NonTerminal("unaryExpression");
            unaryExpression.Rule = unaryOperator + unaryExpression | primaryExpression;

            var mulExpression = MakeInfixOperator("mul", mul | div | mod | mod2, unaryExpression);
            var addExpression = MakeInfixOperator("add", plus | minus, mulExpression);
            var concatExpression = MakeInfixOperator("concat", and, addExpression);
            var relExpression = MakeInfixOperator("rel", lt | le | gt | ge, concatExpression);
            var eqExpression = MakeInfixOperator("eq", eq | ne_brackets | ne_exclamation | like, relExpression);


            var negExpression = MakePrefixOperator("boolNeg", bool_not, eqExpression);
            var isExpression = MakeInfixOperator("boolIs", bool_is, negExpression);
            var boolAndAlsoExpression = MakeInfixOperator("boolAndAlso", bool_andAlso, isExpression);
            var boolAndExpression = MakeInfixOperator("boolAnd", bool_and, boolAndAlsoExpression);
            var boolOrElseExpression = MakeInfixOperator("boolOrElse", bool_orElse, boolAndExpression);
            var boolXorExpression = MakeInfixOperator("boolXor", bool_xor, boolOrElseExpression);
            var boolOrExpression = MakeInfixOperator("boolOr", bool_or, boolXorExpression);

            expression.Rule = boolOrExpression + question + expression + colon + expression | boolOrExpression;
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


        private NonTerminal MakePrefixOperator(string name, BnfExpression operators, NonTerminal innerNonTerminal)
        {
            var operatorNonTerminal = new NonTerminal(name + "Operator");
            operatorNonTerminal.Rule = operators;

            return MakePrefixOperator(name, operatorNonTerminal, innerNonTerminal);
        }
        private NonTerminal MakePrefixOperator(string name, NonTerminal operatorNonTerminal, NonTerminal innerNonTerminal)
        {
            var expressionNonTerminal = new NonTerminal(name + "Expression");
            expressionNonTerminal.Rule = /*expressionNonTerminal +*/ operatorNonTerminal + innerNonTerminal | innerNonTerminal;

            return expressionNonTerminal;
        }
    }
}