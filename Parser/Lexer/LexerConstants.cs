namespace Parser.Lexer
{
    internal static class LexerConstants
    {
        public const string PARANTHESES_OPEN = "(";
        public const string PARANTHESES_CLOSE = ")";
        public const string ACCOLADES_OPEN = "{";
        public const string ACCOLADES_CLOSE = "}";
        public const string BACKETS_OPEN = "[";
        public const string BACKETS_CLOSE = "]";

        public const string END_OF_STATEMENT = ";";

        /// Math signs
        public const string PRECENDENCE_BEGIN_SIGN = "(";
        public const string PRECENDENCE_END_SIGN = ")";
        public const string PLUS_SIGN = "+";
        public const string MINUS_SIGN = "-";
        public const string TIMES_SIGN = "*";
        public const string DIVIDE_SIGN = "/";
        public const string MODULO_SIGN = "%";

        public const string GREATER_THAN_SIGN = ">";
        public const string GREATER_THAN_EQUAL_SIGN = ">=";
        public const string LESS_THAN_SIGN = "<";
        public const string LESS_THAN_EQUAL_SIGN = "<=";
        public const string EQUIVALENT_SIGN = "==";
        public const string EQUALS_SIGN = "===";
        public const string NOT_EQUIVALENT_SIGN = "!=";
        public const string NOT_EQUALS_SIGN = "!==";
        public const string ASSIGN_OPERATOR = "=";

        public const string NOT_SIGN = "!";

        public const string TERNIARY_OPERATOR_TRUE = "?";
        public const string TERNIARY_OPERATOR_FALSE = ":";

        public const string HEX_SIGN = "0x";
        public const char DECIMAL_SEPARATOR_SIGN = '.';
        public const string NUMBER_INDENTATION = "_";

        public const string SINGLE_QOUTE = "\'";
        public const string DOUBLE_QOUTE = "\"";

        /// Explicit type indicators
        public const char DOUBLE_INDICATOR = 'd';
        public const char FLOAT_INDICATOR = 'f';

        public const char CHARACTER_INDICATOR = 'c';
        public const char STRING_INDICATOR = 's';

        public const string COMMENT_INDICATOR = "//";
        public const string SUMMARY_INDICATOR = "///";

        public static class KeyWords
        {
            public const string VARIABLE_TYPE_INFERRED = "var";
            public const string PUBLIC = "public";
            public const string PROTECTED = "protected";
            public const string INTERNAL = "internal";
            public const string PRIVATE = "private";

            //types ? todo: is this the right moment and place?
            public const string DOUBLE = "double";
            public const string FLOAT = "float";
            public const string INTEGER = "int";

            public const string Float = "float";
            public const string Integer = "integer";
            public const string String = "string";
            public const string Character = "char";
        }

        public static class OperatorPrecedence
        {
            private static IReadOnlyDictionary<string, int> _precendences = new Dictionary<string, int>
            {
                [LESS_THAN_SIGN] = 10,
                [LESS_THAN_EQUAL_SIGN] = 10,
                [PLUS_SIGN] = 20,
                [MINUS_SIGN] = 20,
                [TIMES_SIGN] = 40,
                [DIVIDE_SIGN] = 40,
            };

            public static int Get(Token token)
            {
                return Get(token.Name);
            }

            public static int Get(string @operator)
            {
                const int DEFAULT_OPERATOR_PRECENDECE = -1;
                return _precendences.TryGetValue(@operator, out var result) ? result : DEFAULT_OPERATOR_PRECENDECE;
            }
        }

        public static IDictionary<string, TokenType> PredefinedKeyWords = new Dictionary<string, TokenType>()
        {
            { KeyWords.VARIABLE_TYPE_INFERRED, TokenType.VariableTypeInferred },
            { KeyWords.PUBLIC, TokenType.PublicScope},
            { KeyWords.INTERNAL, TokenType.InternalScope },
            { KeyWords.PROTECTED, TokenType.ProtectedScope },
            { KeyWords.PRIVATE, TokenType.PrivateScope },

            //types ? todo: is this the right moment and place?
            { KeyWords.DOUBLE, TokenType.Double },
            { KeyWords.Float, TokenType.Float },
            { KeyWords.Integer, TokenType.Integer },
            { KeyWords.String, TokenType.String },
            { KeyWords.Character, TokenType.Character },


        };
    }
}
