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
    [Language("SSISExpression", "1", "SSIS Expressions")]
    public class ExpressionGrammar : Grammar
    {
        private IdentifierTerminal CreateSsisIdentifier()
        {
            var id = new IdentifierTerminal("identifier");
            StringLiteral term = new StringLiteral("identifier_qouted");
            term.AddStartEnd("[", "]", StringOptions.NoEscapes);
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

        private NonTerminal _variableNonTerminal;
        private NonTerminal _columnNonTerminal;

        public BnfTerm VariableTerm
        {
            get
            {
                return _variableNonTerminal;
            }
        }

        public BnfTerm ColumnTerm
        {
            get
            {
                return _columnNonTerminal;
            }
        }

        public ExpressionGrammar()
            : base(false)
        {
            // Tokens
            var lParen = ToTerm("(");
            var rParen = ToTerm(")");
            var comma = ToTerm(",");

            var plus = ToTerm("+");
            var minus = ToTerm("-");
            var mul = ToTerm("*");
            var div = ToTerm("/");
            var mod = ToTerm("%");
            var and = ToTerm("&");
            var or = ToTerm("|");
            var xor = ToTerm("^");
            var bool_or = ToTerm("||");
            var bool_and = ToTerm("&&");

            var asgn = ToTerm("=");

            var eq = ToTerm("==");
            var ne = ToTerm("!=");
            var lt = ToTerm("<");
            var le = ToTerm("<=");
            var gt = ToTerm(">");
            var ge = ToTerm(">=");

            var not = ToTerm("!");
            var inv = ToTerm("~");

            var question = ToTerm("?");
            var colon = ToTerm(":");

            // Keywords
            var dt_i1 = ToTerm("DT_I1");
            var dt_i2 = ToTerm("DT_I2");
            var dt_i4 = ToTerm("DT_I4");
            var dt_i8 = ToTerm("DT_I8");
            var dt_r4 = ToTerm("DT_R4");
            var dt_r8 = ToTerm("DT_R8");
            var dt_cy = ToTerm("DT_CY");
            var dt_date = ToTerm("DT_DATE");
            var dt_bool = ToTerm("DT_BOOL");
            var dt_numeric = ToTerm("DT_NUMERIC");
            var dt_decimal = ToTerm("DT_DECIMAL");
            var dt_ui1 = ToTerm("DT_UI1");
            var dt_ui2 = ToTerm("DT_UI2");
            var dt_ui4 = ToTerm("DT_UI4");
            var dt_ui8 = ToTerm("DT_UI8");
            var dt_guid = ToTerm("DT_GUID");
            var dt_bytes = ToTerm("DT_BYTES");
            var dt_str = ToTerm("DT_STR");
            var dt_wstr = ToTerm("DT_WSTR");
            var dt_dbdate = ToTerm("DT_DBDATE");
            var dt_dbtime = ToTerm("DT_DBTIME");
            var dt_dbtime2 = ToTerm("DT_DBTIME2");
            var dt_dbtimestamp = ToTerm("DT_DBTIMESTAMP");
            var dt_dbtimestamp2 = ToTerm("DT_DBTIMESTAMP2");
            var dt_dbtimestampoffset = ToTerm("DT_DBTIMESTAMPOFFSET");
            var dt_filetime = ToTerm("DT_FILETIME");
            var dt_image = ToTerm("DT_IMAGE");
            var dt_text = ToTerm("DT_TEXT");
            var dt_ntext = ToTerm("DT_NTEXT");
            var nullKeyword = ToTerm("NULL");

            var identifier = CreateSsisIdentifier();

            var columnIdentifier = CreateTerm("columnIdentifier");

            //TODO: space not allowed between @ and var name
            var variable = new NonTerminal("variable");
            variable.Rule = ToTerm("@") + identifier;
            _variableNonTerminal = variable;

            var column = new NonTerminal("column");
            column.Rule = ToTerm("#") + columnIdentifier;
            _columnNonTerminal = column;

            // Literals
            //TODO: suffixes
            var number = new NumberLiteral("number");
            number.AddPrefix("0x", NumberOptions.Hex);
            var stringLiteral = new StringLiteral("string", "\"", StringOptions.AllowsDoubledQuote);

            // Types
            var type = new NonTerminal("type");
            type.Rule = dt_i1 | dt_i2 | dt_i4 | dt_i8 | dt_cy | dt_date | dt_bool | dt_ui1 | dt_ui2 | dt_ui4 | dt_ui8 |
                dt_guid | dt_dbdate | dt_dbtime | dt_dbtimestamp |
                dt_filetime | dt_image | dt_ntext;

            var typeGeneric = new NonTerminal("typeGeneric");
            typeGeneric.Rule = dt_str | dt_wstr | dt_bytes | dt_decimal | dt_numeric | dt_text |
                dt_dbtime2 | dt_dbtimestamp2 | dt_dbtimestampoffset;

            // expressionOrAssignment
            var expressionOrAssignment = new NonTerminal("expressionOrAssignment");
            Root = expressionOrAssignment;

            // Expressions
            var expression = new NonTerminal("expression");

            // Expressions
            var assignment = new NonTerminal("assignment");
            assignment.Rule = variable + asgn + expression;

            // Functions
            var parameters = new NonTerminal("parameters");
            parameters.Rule = MakePlusRule(parameters, comma, expression);

            var nullCall = new NonTerminal("nullCall");
            nullCall.Rule = nullKeyword + lParen + type + rParen;

            var nullCallGeneric = new NonTerminal("nullCallGeneric");
            nullCallGeneric.Rule = nullKeyword + lParen + typeGeneric + comma + parameters + rParen;

            //TODO: space not allowd between identifier and parenthesis
            var functionCall = new NonTerminal("functionCall");
            functionCall.Rule = identifier + (lParen + parameters + rParen | lParen + rParen);

            var cast = new NonTerminal("cast");
            cast.Rule = lParen + type + rParen;

            var castGeneric = new NonTerminal("castGeneric");
            castGeneric.Rule = lParen + typeGeneric + comma + parameters + rParen;

            var primaryExpression = new NonTerminal("primaryExpression");
            primaryExpression.Rule = functionCall | number | stringLiteral | nullCall | nullCallGeneric | identifier | variable | column | lParen + expression + rParen;

            var unaryOperator = new NonTerminal("unaryOperator");
            unaryOperator.Rule = minus | not | inv | cast | castGeneric;

            var unaryExpression = new NonTerminal("unaryExpression");
            unaryExpression.Rule = unaryOperator + unaryExpression | primaryExpression;

            var mulExpression = MakeInfixOperator("mul", mul | div | mod, unaryExpression);
            var addExpression = MakeInfixOperator("add", plus | minus, mulExpression);
            var relExpression = MakeInfixOperator("rel", lt | le | gt | ge, addExpression);
            var eqExpression = MakeInfixOperator("eq", eq | ne, relExpression);

            var andExpression = MakeInfixOperator("and", and, eqExpression);
            var xorExpression = MakeInfixOperator("xor", xor, andExpression);
            var orExpression = MakeInfixOperator("or", or, xorExpression);

            var boolAndExpression = MakeInfixOperator("boolAnd", bool_and, orExpression);
            var boolOrExpression = MakeInfixOperator("boolOr", bool_or, boolAndExpression);

            expression.Rule = boolOrExpression + question + expression + colon + expression | boolOrExpression;

            // TODO: handle dataflow from expression to the assignee
            expressionOrAssignment.Rule = expression | assignment;
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
