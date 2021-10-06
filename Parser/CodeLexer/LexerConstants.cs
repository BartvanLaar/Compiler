namespace Parser.CodeLexer
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
        public const string PRECENDENCE_BEGIN = "(";
        public const string PRECENDENCE_END = ")";
        public const string PLUS = "+";
        public const string PLUS_ASSIGN = "+=";
        public const string MINUS = "-";
        public const string MINUS_ASSIGN = "-=";
        public const string TIMES = "*";
        public const string TIMES_ASSIGN = "*=";
        public const string DIVIDE = "/";
        public const string DIVIDE_ASSIGN = "/=";
        public const string MODULO = "%";
        public const string MODULO_ASSIGN = "%=";

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
            public const string VARIABLE_DECLARATION = "decl";
            public const string FUNCTION_DEFINITION = "def";
            public const string PUBLIC = "public";
            public const string PROTECTED = "protected";
            public const string INTERNAL = "internal";
            public const string PRIVATE = "private";

            //types ? todo: is this the right moment and place?
            public const string DOUBLE = "double";
            public const string FLOAT = "float";
            public const string INTEGER = "int";
            public const string STRING = "string";
            public const string CHARACTER = "char";
        }

        public static class OperatorPrecedence
        {
            private static IReadOnlyDictionary<TokenType, int> _precendences = new Dictionary<TokenType, int>
            {
                [TokenType.Equivalent] = 10,
                [TokenType.Equals] = 10,
                [TokenType.LessThan] = 10,
                [TokenType.LessThanOrEqualTo] = 10,
                [TokenType.GreaterThan] = 10,
                [TokenType.GreaterThanOrEqualTo] = 10,
                [TokenType.Add] = 20,
                [TokenType.AddAssign] = 20,
                [TokenType.Subtract] = 20,
                [TokenType.SubtractAssign] = 20,
                [TokenType.Multiply] = 40,
                [TokenType.MultiplyAssign] = 40,
                [TokenType.Divide] = 40,
                [TokenType.DivideAssign] = 40,
            };

            public static int Get(Token token)
            {
                return Get(token.TokenType);
            }

            public static int Get(TokenType tokenType)
            {
                const int DEFAULT_OPERATOR_PRECENDECE = -1;
                return _precendences.TryGetValue(tokenType, out var result) ? result : DEFAULT_OPERATOR_PRECENDECE;
            }
        }

        public static IDictionary<string, TokenType> PredefinedKeyWords = new Dictionary<string, TokenType>()
        {
            { KeyWords.VARIABLE_TYPE_INFERRED_1, TokenType.VariableDeclaration },
            { KeyWords.VARIABLE_TYPE_INFERRED_2, TokenType.VariableDeclaration },
            { KeyWords.VARIABLE_DECLARATION, TokenType.VariableDeclaration },
            { KeyWords.FUNCTION_DEFINITION, TokenType.FunctionDefinition },

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


        };
    }
}
