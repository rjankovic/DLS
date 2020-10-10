using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace CD.DLS.Parse.Mssql.Ssas
{
    /// <summary>
    /// MDX grammar for Irony (https://irony.codeplex.com/) based on MDXGrammar.txt (taken from https://github.com/drywolf/mdx-grammar)
    /// </summary>
    [Language("MDX", "1", "MDX grammar")]
    public class MdxGrammar : Grammar
    {
        public MdxGrammar() : base(false)
        {
            var comment = new CommentTerminal("comment", "/*", true, "*/");
            var lineComment = new CommentTerminal("line_comment", "--", "\n", "\r\n");
            var lineCommentSlash = new CommentTerminal("line_comment", "//", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);
            NonGrammarTerminals.Add(lineComment);
            NonGrammarTerminals.Add(lineCommentSlash);

            var number = new NumberLiteral("number");
            var string_literal = new StringLiteral("string", "'", StringOptions.AllowsDoubledQuote);
            var T__53 = ToTerm("=");
            var T__54 = ToTerm("USE_EQUAL_ALLOCATION");
            var T__55 = ToTerm("USE_EQUAL_INCREMENT");
            var T__56 = ToTerm("USE_WEIGHTED_ALLOCATION");
            var T__57 = ToTerm("USE_WEIGHTED_INCREMENT");
            var T__58 = ToTerm(",");
            var T__59 = ToTerm("*");
            var T__60 = ToTerm(".");
            var T__61 = ToTerm("(");
            var T__62 = ToTerm(")");
            var T__63 = ToTerm("<>");
            var T__64 = ToTerm("<");
            var T__65 = ToTerm(">");
            var T__66 = ToTerm("<=");
            var T__67 = ToTerm(">=");
            var T__68 = ToTerm("+");
            var T__69 = ToTerm("-");
            var T__70 = ToTerm("/");
            var T__71 = ToTerm("^");
            var T__72 = ToTerm(":");
            var T__73 = ToTerm("&");
            var T__74 = ToTerm("{");
            var T__75 = ToTerm("}");
            var T__76 = ToTerm("!");
            var T__77 = ToTerm(";");
            var AND = ToTerm("AND");
            var AS = ToTerm("AS");
            var BY = ToTerm("BY");
            var CASE = ToTerm("CASE");
            var CALCULATED = ToTerm("CALCULATED");
            var CALCULATION = ToTerm("CALCULATION");
            var CELL = ToTerm("CELL");
            var CELL_ORDINAL = ToTerm("CELL_ORDINAL");
            var CREATE = ToTerm("CREATE");
            var CUBE = ToTerm("CUBE");
            var DIMENSION = ToTerm("DIMENSION");
            var ELSE = ToTerm("ELSE");
            var EMPTY = ToTerm("EMPTY");
            var END = ToTerm("END");
            var FORMATTED_VALUE = ToTerm("FORMATTED_VALUE");
            var FOR = ToTerm("FOR");
            var FROM = ToTerm("FROM");
            var SELECT = ToTerm("SELECT");
            var IS = ToTerm("IS");
            var GLOBAL = ToTerm("GLOBAL");
            var MEMBER = ToTerm("MEMBER");
            var NON = ToTerm("NON");
            var NOT = ToTerm("NOT");
            var ON = ToTerm("ON");
            var OR = ToTerm("OR");
            var PROPERTIES = ToTerm("PROPERTIES");
            var SESSION = ToTerm("SESSION");
            var SET = ToTerm("SET");
            var IF = ToTerm("IF");
            var THEN = ToTerm("THEN");
            var UPDATE = ToTerm("UPDATE");
            var VALUE = ToTerm("VALUE");
            var USE_EQUAL_ALLOCATION = ToTerm("USE_EQUAL_ALLOCATION");
            var USE_EQUAL_INCREMENT = ToTerm("USE_EQUAL_INCREMENT");
            var USE_WEIGHTED_ALLOCATION = ToTerm("USE_WEIGHTED_ALLOCATION");
            var USE_WEIGHTED_INCREMENT = ToTerm("USE_WEIGHTED_INCREMENT");
            var VISUAL = ToTerm("VISUAL");
            var WITH = ToTerm("WITH");
            var WHEN = ToTerm("WHEN");
            var WHERE = ToTerm("WHERE");
            var XOR = ToTerm("XOR");
            var ROWS = ToTerm("ROWS");
            var COLUMNS = ToTerm("COLUMNS");
            var SCOPE = ToTerm("SCOPE");
            var EXISTING = ToTerm("EXISTING");
            var ALTER = ToTerm("ALTER");
            var DYNAMIC = ToTerm("DYNAMIC");
            var HIDDEN = ToTerm("HIDDEN");
            var DEFAULT_MEMBER = ToTerm("DEFAULT_MEMBER");
            var CALCULATE = ToTerm("CALCULATE");
            var HAVING = ToTerm("HAVING");
            var FLOAT = new NumberLiteral("FLOAT");
            var INTEGER = new NumberLiteral("INTEGER", NumberOptions.IntOnly);
            var Id_simple = TerminalFactory.CreateSqlExtIdentifier(this, "id_simple"); //covers normal identifiers (abc) and quoted id's ([abc d], "abc d")
            var Id_unquoted = TerminalFactory.CreateSqlUnquotedIdentifier(this, "id_unquoted");


            var mdx_script = new NonTerminal("mdx_script");
            var mdx_statement_list = new NonTerminal("mdx_statement_list");
            var mdx_statement_single = new NonTerminal("mdx_statement_single");
            var update_statement = new NonTerminal("update_statement");
            var weight_value_expression = new NonTerminal("weight_value_expression");
            var condition = new NonTerminal("condition");
            var select_statement = new NonTerminal("select_statement");
            var select_statement_subcube = new NonTerminal("select_statement_subcube");
            var with_clause_single = new NonTerminal("with_clause_single");
            var member_name = new NonTerminal("member_name");
            var property_definition = new NonTerminal("property_definition");
            var set_name = new NonTerminal("set_name");
            var compound_id = new NonTerminal("compound_id");
            var axis_specification = new NonTerminal("axis_specification");
            var axis_name = new NonTerminal("axis_name");
            var property = new NonTerminal("property");
            var cube_specification = new NonTerminal("cube_specification");
            //var cube_name = new NonTerminal("cube_name");
            var cell_property = new NonTerminal("cell_property");
            var provider_specific_cell_property = new NonTerminal("provider_specific_cell_property");
            var expression = new NonTerminal("expression");
            var expression_or_xor = new NonTerminal("expression_or_xor");
            var expression_and = new NonTerminal("expression_and");
            var expression_compare = new NonTerminal("expression_compare");
            var expression_add = new NonTerminal("expression_add");
            var expression_mult = new NonTerminal("expression_mult");
            var expression_power = new NonTerminal("expression_power");
            var expression_unary = new NonTerminal("expression_unary");
            var expression_range_is = new NonTerminal("expression_range_is");
            var expression_property = new NonTerminal("expression_property");
            var expression_simple = new NonTerminal("expression_simple");
            var expressions_list = new NonTerminal("expressions_list");
            var expression_function = new NonTerminal("expression_function");
            var expression_case = new NonTerminal("expression_case");
            var when_clause = new NonTerminal("when_clause");
            //var t = new RegexBasedTerminal()
            var identifier = new NonTerminal("identifier");
            //var unquoted_identifier = new NonTerminal("unquoted_identifier");
            //var quoted_identifier = new NonTerminal("quoted_identifier");
            var keyword = new NonTerminal("keyword");
            var create_calculation_statement = new NonTerminal("create_calculation_statement");
            var scope_statement = new NonTerminal("scope_statement");
            var alter_statement = new NonTerminal("alter_statement");
            var calculate_statement = new NonTerminal("calculate_statement");
            var having_clause = new NonTerminal("having_clause");

            identifier.Rule = Id_simple;
            compound_id.Rule = MakePlusRule(compound_id, T__60, Id_simple);


            //mdx_statement: mdx_statement_single EOF;
            //mdx_statement_single: (select_statement | update_statement);
            this.Root = mdx_script;

            mdx_script.Rule = mdx_statement_list
                | mdx_statement_list + T__77;

            mdx_statement_list.Rule = MakePlusRule(mdx_statement_list, T__77, mdx_statement_single);


            mdx_statement_single.Rule = select_statement
                | update_statement
                | create_calculation_statement
                | property_definition
                | scope_statement
                | alter_statement
                | calculate_statement;
            //| T__61 + property_definition + T__62;

            calculate_statement.Rule = CALCULATE;

            //update_statement: UPDATE(CUBE) ? cube_specification SET expression_property '=' expression_or_xor('USE_EQUAL_ALLOCATION' | 'USE_EQUAL_INCREMENT' | 'USE_WEIGHTED_ALLOCATION'(BY weight_value_expression) ? 'USE_WEIGHTED_INCREMENT'(BY weight_value_expression) ? ) ? ;
            var update_cube = new NonTerminal("update_cube");
            update_cube.Rule = UPDATE + CUBE | UPDATE;
            var allocation_specification = new NonTerminal("allocation_specification");
            allocation_specification.Rule = USE_EQUAL_ALLOCATION | USE_EQUAL_INCREMENT | USE_WEIGHTED_ALLOCATION
                | USE_EQUAL_ALLOCATION + weight_value_expression | USE_EQUAL_INCREMENT + weight_value_expression | USE_WEIGHTED_ALLOCATION + weight_value_expression;
            var weighted_increment_specificaiton = new NonTerminal("weighted_increment_specification");
            weighted_increment_specificaiton.Rule = USE_WEIGHTED_INCREMENT | USE_WEIGHTED_INCREMENT + BY + weight_value_expression;
            update_statement.Rule = update_cube + cube_specification + SET + expression_property + "=" + expression_or_xor + allocation_specification + weighted_increment_specificaiton
                | update_cube + cube_specification + SET + expression_property + "=" + expression_or_xor;

            var alter_cube = new NonTerminal("alter_cube");
            alter_cube.Rule = ALTER + CUBE;
            alter_statement.Rule = alter_cube + cube_specification + UPDATE + DIMENSION + compound_id + T__58 + DEFAULT_MEMBER + T__53 + expression;


            //weight_value_expression: expression_property;
            weight_value_expression.Rule = expression_property;

            //condition: expression_property;
            // DEVIATION!
            condition.Rule = expression;
            //condition.Rule = expression_property;

            //select_statement: (WITH(with_clause_single) + ) ? select_statement_subcube((CELL) ? PROPERTIES cell_property(',' cell_property) * ) ? ;
            var with_clause_list = new NonTerminal("with_clasue_list");
            with_clause_list.Rule = MakePlusRule(with_clause_list, Empty, with_clause_single);
            var with_clause = new NonTerminal("with_clause");
            with_clause.Rule = WITH + with_clause_list | WITH;
            var cell_properties_list = new NonTerminal("cell_properties_list");
            cell_properties_list.Rule = MakePlusRule(cell_properties_list, T__58, cell_property);
            var cell_properties_clause = new NonTerminal("cell_properties_clause");
            cell_properties_clause.Rule = CELL + PROPERTIES + cell_properties_list
                | PROPERTIES + cell_properties_list;
            select_statement.Rule = with_clause + select_statement_subcube + cell_properties_clause
                | select_statement_subcube + cell_properties_clause
                | with_clause + select_statement_subcube
                | select_statement_subcube;

            //select_statement_subcube: SELECT('*' | axis_specification(',' axis_specification) * ) ? FROM cube_specification(WHERE condition) ? ;
            var axis_specification_list = new NonTerminal("axis_specification_list");
            axis_specification_list.Rule = MakePlusRule(axis_specification_list, T__58, axis_specification);
            var axis_or_star_specification = new NonTerminal("axis_or_star_specification");
            axis_or_star_specification.Rule = T__59
                | axis_specification_list;
            var from_clause = new NonTerminal("from_clause");
            from_clause.Rule = FROM + cube_specification
                | FROM + cube_specification + WHERE + condition;
            select_statement_subcube.Rule = SELECT + axis_or_star_specification + from_clause;

            //with_clause_single: (((CALCULATED) ? MEMBER member_name AS | CELL CALCULATION FOR expression AS ) expression(',' property_definition) * | SET set_name AS expression );
            var calculated_member_declaration = new NonTerminal("calculated_member_declaration");
            calculated_member_declaration.Rule = CALCULATED + MEMBER + member_name + AS
                | MEMBER + member_name + AS;
            var cell_calcuation_declaration = new NonTerminal("cell_calculation_declaration");
            cell_calcuation_declaration.Rule = CELL + CALCULATION + FOR + expression + AS;
            var calculated_member_or_cell_calculation_declaration = new NonTerminal("calculated_member_or_function_declaration");
            calculated_member_or_cell_calculation_declaration.Rule = calculated_member_declaration | cell_calcuation_declaration;
            var property_definition_list = new NonTerminal("property_definition_list");
            property_definition_list.Rule = MakePlusRule(property_definition_list, T__58, property_definition);
            var calculated_member_definition = new NonTerminal("calculated_member_definition");
            calculated_member_definition.Rule = calculated_member_or_cell_calculation_declaration + expression + T__58 + property_definition_list
                | calculated_member_or_cell_calculation_declaration + expression;
            var calculated_set_definition = new NonTerminal("calculated_set_definition");
            calculated_set_definition.Rule =
                SET + set_name + AS + expression
                | DYNAMIC + SET + set_name + AS + expression
                | DYNAMIC + SET + set_name + AS + expression + T__58 + property_definition_list
                | HIDDEN + SET + set_name + AS + expression
                | HIDDEN + DYNAMIC + SET + set_name + AS + expression
                | HIDDEN + DYNAMIC + SET + set_name + AS + expression + T__58 + property_definition_list;
            with_clause_single.Rule = calculated_member_definition | calculated_set_definition;

            //member_name: compound_id;
            member_name.Rule = compound_id;
            //var property_definition_inner = new NonTerminal("property_definition_inner");

            var property_scope_specification_part = new NonTerminal("property_scope_specification_part");
            property_scope_specification_part.Rule = compound_id
                | T__74 + expressions_list + T__75;
            var property_scope_specification = new NonTerminal("property_scope_specificatoin");
            property_scope_specification.Rule = MakePlusRule(property_scope_specification, T__58, property_scope_specification_part);

            //property_definition: identifier '=' expression_or_xor;
            property_definition.Rule = //identifier + T__53 + expression_or_xor
                                       //| 
                                       //identifier + T__61 + identifier + T__62 + T__53 + expression_or_xor
                                       //| 
                                       //T__61 + member_name + T__53 + expression_or_xor + T__62
                                       //| 
                expression_function + T__53 + expression_or_xor
                | identifier + T__53 + expression_or_xor
                | T__61 + compound_id + T__53 + expression_or_xor + T__62
                | T__61 + /*compound_id*/ property_scope_specification + T__62 + T__53 + expression_or_xor
                | IF + expression + THEN + expression + END + IF
                | IF + expression + THEN + expression + ELSE + expression + END + IF;
            //| T__61 + compound_id + T__53 + identifier + T__62;

            //property_definition.Rule = property_definition_inner;
            //| property_definition_inner;

            //set_name: compound_id;
            set_name.Rule = compound_id; // identifier;

            //compound_id: identifier('.' identifier) * ;
            //compound_id.Rule = MakePlusRule(compound_id, T__60, identifier);

            //axis_specification: (NON EMPTY )? expression((DIMENSION) ? PROPERTIES property(',' property) * ) ? ON axis_name;
            var properties_list = new NonTerminal("properties_list");
            properties_list.Rule = MakePlusRule(properties_list, T__58, property);
            var dimension_properties_clause = new NonTerminal("properties_clause");
            dimension_properties_clause.Rule = DIMENSION + PROPERTIES + properties_list
                | PROPERTIES + properties_list;
            axis_specification.Rule =
                NON + EMPTY + expression + dimension_properties_clause + ON + axis_name
                | expression + dimension_properties_clause + ON + axis_name
                | NON + EMPTY + expression + ON + axis_name
                | expression + ON + axis_name

                | NON + EMPTY + expression + having_clause + dimension_properties_clause + ON + axis_name
                | expression + having_clause + dimension_properties_clause + ON + axis_name
                | NON + EMPTY + expression + having_clause + ON + axis_name
                | expression + having_clause + ON + axis_name;

            having_clause.Rule = HAVING + expression;

            //axis_name: (identifier | INTEGER);
            //axis_name.Rule = identifier | INTEGER;
            axis_name.Rule = ROWS | COLUMNS | INTEGER;



            //property: compound_id;
            property.Rule = compound_id; // compound_id;

            //cube_specification: (cube_name | (NON VISUAL )? '(' select_statement_subcube ')' );
            cube_specification.Rule = compound_id
                | NON + VISUAL + T__61 + select_statement_subcube + T__62
                | T__61 + select_statement_subcube + T__62;

            //cube_name: (compound_id | RANET_EXPRESSION);
            //cube_name.Rule = identifier; //compound_id;  //| RANET_EXPRESSION;

            //cell_property: (CELL_ORDINAL | VALUE | FORMATTED_VALUE | provider_specific_cell_property);
            cell_property.Rule = CELL_ORDINAL | VALUE | FORMATTED_VALUE | provider_specific_cell_property;

            //provider_specific_cell_property: identifier;
            provider_specific_cell_property.Rule = identifier;

            //expression: expression_or_xor;
            expression.Rule = expression_or_xor
                | IF + expression + THEN + expression + END + IF
                | IF + expression + THEN + expression + ELSE + expression + END + IF;

            //expression_or_xor: expression_and((XOR | OR) expression_and) * ;
            var or_xor = new NonTerminal("or_xor");
            or_xor.Rule = OR | XOR;
            expression_or_xor.Rule = MakePlusRule(expression_or_xor, or_xor, expression_and);

            //expression_and: expression_compare(AND expression_compare) * ;
            expression_and.Rule = MakePlusRule(expression_and, AND, expression_compare);

            //expression_compare: expression_add(('=' | '<>' | '<' | '>' | '<=' | '>=') expression_add) * ;
            var comparison_operator = new NonTerminal("comparison_operator");
            comparison_operator.Rule = T__53 | T__63 | T__64 | T__65 | T__66 | T__67;
            expression_compare.Rule = MakePlusRule(expression_compare, comparison_operator, expression_add);

            //expression_add: expression_mult(('+' | '-') expression_mult) * ;
            var add_operator = new NonTerminal("add_operator");
            add_operator.Rule = T__68 | T__69;
            expression_add.Rule = MakePlusRule(expression_add, add_operator, expression_mult);

            //expression_mult: expression_power(('/' | '*') expression_power) * ;
            var mult_operator = new NonTerminal("mult_operator");
            mult_operator.Rule = T__70 | T__59;
            expression_mult.Rule = MakePlusRule(expression_mult, mult_operator, expression_power);

            //expression_power: expression_unary('^' expression_unary) * ;
            expression_power.Rule = MakePlusRule(expression_power, T__71, expression_unary);

            //expression_unary: ('-' expression_range_is | '+' expression_range_is | NOT expression_range_is | expression_range_is );
            expression_unary.Rule = T__69 + expression_range_is | T__68 + expression_range_is | NOT + expression_range_is | expression_range_is;

            //expression_range_is: expression_property(':' expression_property | IS expression_property) ? ;
            expression_range_is.Rule = expression_property + T__72 + expression_property
                | expression_property + IS + expression_property
                | expression_property;

            // expression_property ~ [a].&[b]&[c] (among others)

            //expression_property: expression_simple('.'(unquoted_identifier | '&' quoted_identifier('&' quoted_identifier) * | quoted_identifier | expression_function)) * ;
            var member_identifier_part_list = new NonTerminal("member_identifier_part_list");
            member_identifier_part_list.Rule = MakePlusRule(member_identifier_part_list, T__73, Id_simple);
            var member_identifier = new NonTerminal("member_identifier");
            member_identifier.Rule = T__60 + Id_simple | T__60 + T__73 + member_identifier_part_list | T__60 + expression_function;  // <-- what is this? // identifier | T__73 + member_identifier_part_list | expression_function;
            var member_identifiers_list = new NonTerminal("member_identifiers_list");
            member_identifiers_list.Rule = MakePlusRule(member_identifiers_list, member_identifier);

            expression_property.Rule = expression_simple + member_identifiers_list
                | expression_simple
                | EXISTING + expression_simple + member_identifiers_list
                | EXISTING + expression_simple;
            //                           //| 
            //    expression_simple;

            //expression_simple: (expression_function | '(' expressions_list ')' | '{'(expressions_list) ? '}' | expression_case | STRING | INTEGER | FLOAT | identifier | RANET_EXPRESSION );
            expression_simple.Rule = expression_function
                | T__61 + expressions_list + T__62
                | T__74 + T__75
                | T__74 + expressions_list + T__75
                | expression_case
                | string_literal
                | INTEGER
                | FLOAT
                | identifier;
            //| T__74 + expression_simple + T__72 + expression_simple + T__75;

            //expressions_list: expression(',' expression) * ;
            //var expression_or_empty = new NonTerminal("expression_or_empty");
            //expression_or_empty.Rule = expression
            //    | Empty;
            var comma_list = new NonTerminal("comma_list");
            comma_list.Rule = MakePlusRule(comma_list, T__58);
            expressions_list.Rule = MakePlusRule(expressions_list, comma_list, expression);

            //expression_function: identifier('!' identifier) * '('(expressions_list) ? ')' ;
            var exclamation_identifier_list = new NonTerminal("exclamation_identifier_list");
            exclamation_identifier_list.Rule = MakePlusRule(exclamation_identifier_list, T__76, identifier);
            expression_function.Rule =
                //identifier + T__61 + T__62
                //| identifier + T__61 + expressions_list + T__62
                //| 
                exclamation_identifier_list + T__61 + expressions_list + T__62
                | exclamation_identifier_list + T__61 + T__62;

            //expression_case: CASE(expression) ? (when_clause(when_clause) * ) ? (ELSE expression )? END;
            var when_list = new NonTerminal("when_list");
            when_list.Rule = MakePlusRule(when_list, when_clause);
            var case_body = new NonTerminal("case_body");
            case_body.Rule = when_list + ELSE + expression //+ END
                | when_list// + END
                | ELSE + expression; // + END
                                     //| END;
            expression_case.Rule = CASE + expression + case_body + END
                | CASE + case_body + END;

            //when_clause: WHEN expression THEN expression;
            when_clause.Rule = WHEN + expression + THEN + expression;

            //identifier: (unquoted_identifier | quoted_identifier);

            //identifier.Rule = Id | keyword; // unquoted_identifier; //| quoted_identifier;
            //identifier.Rule = Id_simple | keyword; // unquoted_identifier; //| quoted_identifier;

            //unquoted_identifier: (ID | keyword);
            //unquoted_identifier.Rule = Id_simple | keyword;

            //quoted_identifier: QUOTED_ID;

            //quoted_identifier.Rule = unquoted_identifier;

            //keyword: (DIMENSION | PROPERTIES);
            keyword.Rule = DIMENSION | PROPERTIES;


            create_calculation_statement.Rule = CREATE + calculated_member_definition | CREATE + calculated_set_definition;


            //            SCOPE(Subcube_Expression)
            //              [MDX_Statement]
            //            END SCOPE

            scope_statement.Rule = SCOPE + /*T__61 +*/ expression_property /*+ T__62*/ + T__77 + END + SCOPE
                | SCOPE + /*T__61 +*/ expression_property /*+ T__62*/ + T__77 + mdx_statement_list + T__77 + END + SCOPE
                | SCOPE + /*T__61 +*/ expression_property /*+ T__62*/ + T__77 + mdx_statement_list + T__77; // + END + SCOPE;

            /*

            mdx_statement: mdx_statement_single EOF;
            mdx_statement_single: (select_statement | update_statement);
            update_statement: UPDATE(CUBE) ? cube_specification SET expression_property '=' expression_or_xor('USE_EQUAL_ALLOCATION' | 'USE_EQUAL_INCREMENT' | 'USE_WEIGHTED_ALLOCATION'(BY weight_value_expression) ? 'USE_WEIGHTED_INCREMENT'(BY weight_value_expression) ? ) ? ;
            weight_value_expression: expression_property;
            condition: expression_property;
            select_statement: (WITH(with_clause_single) + ) ? select_statement_subcube((CELL) ? PROPERTIES cell_property(',' cell_property) * ) ? ;
            select_statement_subcube: SELECT('*' | axis_specification(',' axis_specification) * ) ? FROM cube_specification(WHERE condition) ? ;
            with_clause_single: (((CALCULATED) ? MEMBER member_name AS | CELL CALCULATION FOR expression AS ) expression(',' property_definition) * | SET set_name AS expression );
            member_name: compound_id;
            property_definition: identifier '=' expression_or_xor;
            set_name: compound_id;
            compound_id: identifier('.' identifier) * ;
            axis_specification: (NON EMPTY )? expression((DIMENSION) ? PROPERTIES property(',' property) * ) ? ON axis_name;
            axis_name: (identifier | INTEGER);
            property: compound_id;
            cube_specification: (cube_name | (NON VISUAL )? '(' select_statement_subcube ')' );
            cube_name: (compound_id | RANET_EXPRESSION);
            cell_property: (CELL_ORDINAL | VALUE | FORMATTED_VALUE | provider_specific_cell_property);
            provider_specific_cell_property: identifier;
            expression: expression_or_xor;
            expression_or_xor: expression_and((XOR | OR) expression_and) * ;
            expression_and: expression_compare(AND expression_compare) * ;
            expression_compare: expression_add(('=' | '<>' | '<' | '>' | '<=' | '>=') expression_add) * ;
            expression_add: expression_mult(('+' | '-') expression_mult) * ;
            expression_mult: expression_power(('/' | '*') expression_power) * ;
            expression_power: expression_unary('^' expression_unary) * ;
            expression_unary: ('-' expression_range_is | '+' expression_range_is | NOT expression_range_is | expression_range_is );
            expression_range_is: expression_property(':' expression_property | IS expression_property) ? ;
            expression_property: expression_simple('.'(unquoted_identifier | '&' quoted_identifier('&' quoted_identifier) * | quoted_identifier | expression_function)) * ;
            expression_simple: (expression_function | '(' expressions_list ')' | '{'(expressions_list) ? '}' | expression_case | STRING | INTEGER | FLOAT | identifier | RANET_EXPRESSION );
            expressions_list: expression(',' expression) * ;
            expression_function: identifier('!' identifier) * '('(expressions_list) ? ')' ;
            expression_case: CASE(expression) ? (when_clause(when_clause) * ) ? (ELSE expression )? END;
            when_clause: WHEN expression THEN expression;
            identifier: (unquoted_identifier | quoted_identifier);
            unquoted_identifier: (ID | keyword);
            quoted_identifier: QUOTED_ID;
            keyword: (DIMENSION | PROPERTIES);

            */

        }//constructor

    }//class
}//namespace