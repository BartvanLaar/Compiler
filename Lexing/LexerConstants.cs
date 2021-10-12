using System.Collections.Generic;

namespace Lexing
{
    public static class LexerConstants
    {
        public const string PARANTHESES_OPEN = "(";
        public const string PARANTHESES_CLOSE = ")";
        public const string ACCOLADES_OPEN = "{";
        public const string ACCOLADES_CLOSE = "}";
        public const string BACKETS_OPEN = "[";
        public const string BACKETS_CLOSE = "]";

        public const string VARIABLE_SEPARATOR = ",";
        public const string END_OF_STATEMENT = ";";

        /// Math signs
        public const string PRECENDENCE_BEGIN = "(";
        public const string PRECENDENCE_END = ")";
        public const string PLUS = "+";
        public const string PLUS_PLUS = "++";
        public const string PLUS_ASSIGN = "+=";
        public const string MINUS = "-";
        public const string MINUS_MINUS = "--";

        public const string MINUS_ASSIGN = "-=";
        public const string TIMES = "*";
        public const string TIMES_ASSIGN = "*=";
        public const string DIVIDE = "/";
        public const string DIVIDE_ASSIGN = "/=";
        public const string MODULO = "%";
        public const string POWER = "^";

        public const string GREATER_THAN_SIGN = ">";
        public const string GREATER_THAN_EQUAL_SIGN = ">=";
        public const string LESS_THAN_SIGN = "<";
        public const string LESS_THAN_EQUAL_SIGN = "<=";
        public const string EQUIVALENT_SIGN = "==";
        public const string EQUALS_SIGN = "===";
        public const string NOT_EQUIVALENT_SIGN = "!=";
        public const string NOT_EQUALS_SIGN = "!==";
        public const string ASSIGN_OPERATOR = "=";
        public const string NULLABLE_COALESCE = "??";
        public const string NULLABLE_COALESCE_ASSIGN = "??=";
        public const string AND = "&";
        public const string AND_ALSO = "&&";
        public const string OR = "|";
        public const string OR_ELSE = "||";

        public const string RETURN_TYPE_INDICATOR = "->";

        public const string NOT_SIGN = "!";

        public const string TERNIARY_OPERATOR_TRUE = "?";
        public const string TERNIARY_OPERATOR_FALSE = ":";

        public const string HEX_SIGN = "0x";
        public const string DECIMAL_SEPARATOR_SIGN = ".";
        public const string NUMBER_INDENTATION = "_";

        public const string SINGLE_QOUTE = "\'";
        public const string DOUBLE_QOUTE = "\"";

        /// Explicit type indicators
        public const char DOUBLE_INDICATOR = 'd';
        public const char FLOAT_INDICATOR = 'f';

        public const string CHARACTER_INDICATOR = "c";

        public const string COMMENT_INDICATOR = "//";
        public const string SUMMARY_INDICATOR = "///";

        public static class KeyWords
        {
            public const string VARIABLE_TYPE_INFERRED_1 = "var";
            public const string VARIABLE_TYPE_INFERRED_2 = "auto";

            public const string FUNCTION_DEFINITION = "func";
            public const string PUBLIC = "public";
            public const string PROTECTED = "protected";
            public const string INTERNAL = "internal";
            public const string PRIVATE = "private";

            public const string PARAMS = "params";
            public const string IMPLEMENTED = "implemented";
            public const string CLASS = "class";
            public const string STRUCT = "struct";

            public const string IF = "if";
            public const string ELSE = "else";

            public const string IS = "is";
            public const string NOT = "not";

            public const string FOR = "for";
            public const string FOREACH = "each";

            public const string WHILE = "while";
            public const string DO = "do";
            public const string CONTINUE = "continue";
            public const string BREAK = "break";
            public const string RETURN = "return";

            public const string DOUBLE = "double";
            public const string FLOAT = "float";
            public const string INTEGER = "int";
            public const string STRING = "string";
            public const string CHARACTER = "char";
            public const string BOOLEAN = "bool";

            public const string TRUE = "true";
            public const string FALSE = "false";
            public const string VOID = "void";
            public const string NULL = "null";
        }

        private static readonly IDictionary<string, TokenType> _predefinedKeywords = new Dictionary<string, TokenType>()
        {
            { KeyWords.VARIABLE_TYPE_INFERRED_1, TokenType.VariableDeclaration },
            { KeyWords.VARIABLE_TYPE_INFERRED_2, TokenType.VariableDeclaration },
            { KeyWords.FUNCTION_DEFINITION, TokenType.FunctionDefinition },

            {KeyWords.FOR, TokenType.For},
            {KeyWords.FOREACH, TokenType.ForEach},
            {KeyWords.WHILE, TokenType.While },
            {KeyWords.DO, TokenType.Do },

            {KeyWords.IF, TokenType.If },
            {KeyWords.ELSE, TokenType.Else },

            { KeyWords.IS, TokenType.Is},
            { KeyWords.NOT, TokenType.Not},

            { KeyWords.CONTINUE, TokenType.Continue},
            { KeyWords.BREAK, TokenType.Break},
            { KeyWords.RETURN, TokenType.Return},

            { KeyWords.PUBLIC, TokenType.PublicScope},
            { KeyWords.INTERNAL, TokenType.InternalScope },
            { KeyWords.PROTECTED, TokenType.ProtectedScope },
            { KeyWords.PRIVATE, TokenType.PrivateScope },

            //types ? todo: is this the right moment and place?
            { KeyWords.DOUBLE, TokenType.Double },
            { KeyWords.FLOAT, TokenType.Float },
            { KeyWords.INTEGER, TokenType.Integer },
            { KeyWords.STRING, TokenType.String },
            { KeyWords.CHARACTER, TokenType.Character },
            { KeyWords.BOOLEAN, TokenType.Boolean },
            { KeyWords.PARAMS, TokenType.Params },

            { KeyWords.TRUE, TokenType.True },
            { KeyWords.FALSE, TokenType.False },
            { KeyWords.VOID, TokenType.Void },
            { KeyWords.NULL, TokenType.Null },

        };

        public static bool IsPredefinedKeyword(string keyword)
        {
            return IsPredefinedKeyword(keyword, out _);
        }

        public static bool IsPredefinedKeyword(string keyword, out TokenType tokenType)
        {
            return _predefinedKeywords.TryGetValue(keyword, out tokenType);
        }

        public static class OperatorPrecedence
        {
            public const int DEFAULT_OPERATOR_PRECEDENCE = 0;
            private static readonly IReadOnlyDictionary<TokenType, int> _precendences = new Dictionary<TokenType, int>
            {
                // @note: values don't matter too much as long as it's +1 everytime and theyre proper... Higher is more important of an operator.
                [TokenType.OrElse] = 1,
                [TokenType.AndAlso] = 2,
                [TokenType.Equivalent] = 3,
                [TokenType.Equals] = 3,
                [TokenType.LessThan] = 3,
                [TokenType.LessThanOrEqualTo] = 3,
                [TokenType.GreaterThan] = 3,
                [TokenType.GreaterThanOrEqualTo] = 3,
                [TokenType.Add] = 4,
                [TokenType.Subtract] = 4,
                [TokenType.Multiply] = 5,
                [TokenType.Divide] = 5,
            };

            public static bool IsLeftAssociated(Token token)
            {
                return IsLeftAssociated(token.TokenType);
            }

            public static bool IsLeftAssociated(TokenType tokenType)
            {
                return tokenType is not TokenType.Power;
            }

            public static int Get(Token token)
            {
                return Get(token.TokenType);
            }

            public static int Get(TokenType tokenType)
            {
                Get(tokenType, out var precedence);
                return precedence;
            }

            public static bool Get(Token token, out int precedence)
            {
                return Get(token.TokenType, out precedence);
            }

            public static bool Get(TokenType tokenType, out int precedence)
            {
                var hasPrec = _precendences.TryGetValue(tokenType, out precedence);
                if (!hasPrec)
                {
                    precedence = DEFAULT_OPERATOR_PRECEDENCE - 1; // everything thats unmapped should be lower than default.
                }

                return hasPrec;
            }
        }


    }
}
