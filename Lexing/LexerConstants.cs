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

        public const string ARGUMENT_SEPARATOR = ",";
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
        public const string MODULO_ASSIGN = "%=";
        public const string POWER = "^";
        //public const string POWER_ASSIGN = "^"; // what should this do haha

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
        public const string XOr = "^";
        public const string OR_ELSE = "||";

        public const string BIT_SHIFT_LEFT = "<<";
        public const string BIT_SHIFT_RIGHT = ">>";

        public const string RETURN_TYPE_INDICATOR = "->";

        public const string NOT_SIGN = "!";

        public const string TERNIARY_OPERATOR_TRUE = "?";
        public const string TERNIARY_OPERATOR_FALSE = ":";

        public const string HEX_SIGN = "0x";
        public const string DOT = ".";
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

        public static class Keywords
        {
            public const string VARIABLE_TYPE_INFERRED_1 = "var";
            public const string VARIABLE_TYPE_INFERRED_2 = "auto";

            public const string IMPORT = "import";

            public const string FUNCTION_DEFINITION = "func";
            public const string EXPORT = "export";
            public const string EXTEND = "extend";
            public const string EXTERN = "extern";

            public const string STATIC = "static";
            public const string CONST = "const";

            public const string PARAMS = "params";
            public const string CLASS = "class";
            public const string ENUM = "enum";
            public const string INTERFACE = "interface";
            public const string NAMESPACE = "namespace";

            public const string IF = "if";
            public const string ELSE = "else";

            public const string IS = "is";
            public const string AS = "as";
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
            { Keywords.VARIABLE_TYPE_INFERRED_1, TokenType.Type },
            { Keywords.VARIABLE_TYPE_INFERRED_2, TokenType.Type },
            { Keywords.FUNCTION_DEFINITION, TokenType.FunctionDefinition },

            {Keywords.FOR, TokenType.For},
            {Keywords.FOREACH, TokenType.ForEach},
            {Keywords.WHILE, TokenType.While },
            {Keywords.DO, TokenType.Do },

            {Keywords.IF, TokenType.If },
            {Keywords.ELSE, TokenType.Else },

            { Keywords.IS, TokenType.Is},
            { Keywords.AS, TokenType.As},
            { Keywords.NOT, TokenType.Not},

            { Keywords.CONTINUE, TokenType.Continue},
            { Keywords.BREAK, TokenType.Break},
            { Keywords.RETURN, TokenType.Return},
            { Keywords.IMPORT, TokenType.ImportStatement },
            { Keywords.EXPORT, TokenType.Export},
            { Keywords.EXTEND, TokenType.Extend },
            { Keywords.EXTERN, TokenType.Extern },

            { Keywords.ENUM, TokenType.Enum },
            { Keywords.CLASS, TokenType.Class },
            { Keywords.INTERFACE, TokenType.Interface },
            { Keywords.NAMESPACE, TokenType.Namespace },
            { Keywords.STATIC, TokenType.Static },
            { Keywords.CONST, TokenType.Constant },

            //types ? todo: is this the right moment and place?
            { Keywords.DOUBLE, TokenType.Type },
            { Keywords.FLOAT, TokenType.Type },
            { Keywords.INTEGER, TokenType.Type },
            { Keywords.STRING, TokenType.Type },
            { Keywords.CHARACTER, TokenType.Type },
            { Keywords.BOOLEAN, TokenType.Type },
            { Keywords.TRUE, TokenType.Value },
            { Keywords.FALSE, TokenType.Value },
            { Keywords.VOID, TokenType.Type },
            { Keywords.PARAMS, TokenType.Params },

        };

        private static readonly IDictionary<string, TypeIndicator> _keywordToTypeIndicatorConversiontable = new Dictionary<string, TypeIndicator>()
        {
            { Keywords.DOUBLE, TypeIndicator.Double },
            { Keywords.FLOAT, TypeIndicator.Float },
            { Keywords.INTEGER, TypeIndicator.Integer },
            { Keywords.STRING, TypeIndicator.String },
            { Keywords.CHARACTER, TypeIndicator.Character },
            { Keywords.BOOLEAN, TypeIndicator.Boolean },
            { Keywords.TRUE, TypeIndicator.Boolean },
            { Keywords.FALSE, TypeIndicator.Boolean },
            { Keywords.VOID, TypeIndicator.Void },
            { Keywords.VARIABLE_TYPE_INFERRED_1, TypeIndicator.Inferred },
            { Keywords.VARIABLE_TYPE_INFERRED_2, TypeIndicator.Inferred },

        };

        public static TypeIndicator ConvertKeywordToTypeIndicator(string keyword)
        {
            _keywordToTypeIndicatorConversiontable.TryGetValue(keyword, out var typeIndicator);
            return typeIndicator;
        }

        public static bool IsPredefinedKeyword(string keyword)
        {
            return IsPredefinedKeyword(keyword, out _);
        }

        public static bool IsPredefinedKeyword(string keyword, out TokenType tokenType)
        {
            return _predefinedKeywords.TryGetValue(keyword, out tokenType);
        }


        public static OperatorMetadata? GetOperatorMetadata(Token token) => GetOperatorMetadata(token.TokenType);
        public static OperatorMetadata? GetOperatorMetadata(TokenType tokenType)
        {
            return OperatorMetadataFactory.Create(tokenType);
        }

        public class OperatorMetadata
        {
            public bool IsCompoundAssignment { get; set; }
            public bool IsLeftAssociated { get; set; }
            public int Precedence { get; set; }
            public TokenType DecompoundedToken { get; internal set; }
        }

        public static class OperatorPrecedence
        {
            internal const int MULTIPLICATIVE = ADDITIVE + 1;
            internal const int ADDITIVE = SHIFT_PRECEDENCE + 1;
            internal const int SHIFT_PRECEDENCE = RELATIONAL_TEST + 1;
            internal const int RELATIONAL_TEST = EQUALITY_TEST + 1;
            internal const int EQUALITY_TEST = Bitwise_AND + 1;
            internal const int Bitwise_AND = Bitwise_OR + 1;
            internal const int Bitwise_OR = CONDITIONAL_AND + 1;
            internal const int CONDITIONAL_AND = CONDITIONAL_XOR + 1;
            internal const int CONDITIONAL_XOR = CONDITIONAL_OR + 1;
            internal const int CONDITIONAL_OR = NULLABLE_COALESCE + 1;
            internal const int NULLABLE_COALESCE = CONDITIONAL_OPERATOR + 1;
            internal const int CONDITIONAL_OPERATOR = ASSIGNMENT_DECLARATION + 1;
            internal const int ASSIGNMENT_DECLARATION = DEFAULT_OPERATOR_PRECEDENCE + 1;
            public const int DEFAULT_OPERATOR_PRECEDENCE = 0;

            private static readonly IReadOnlyDictionary<TokenType, int> _precendences = new Dictionary<TokenType, int>
            {
                // @note: values don't matter too much as long as it's +1 everytime and theyre proper... Higher is more important of an operator.
                // source: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/#operator-precedence
                // # better good stolen than badly thought...
                [TokenType.Modulo] = MULTIPLICATIVE,
                [TokenType.Multiply] = MULTIPLICATIVE,
                [TokenType.Divide] = MULTIPLICATIVE,

                [TokenType.Add] = ADDITIVE,
                [TokenType.Subtract] = ADDITIVE,

                [TokenType.BitShiftLeft] = SHIFT_PRECEDENCE,
                [TokenType.BitShiftRight] = SHIFT_PRECEDENCE,

                [TokenType.LessThan] = RELATIONAL_TEST,
                [TokenType.LessThanEqual] = RELATIONAL_TEST,
                [TokenType.GreaterThan] = RELATIONAL_TEST,
                [TokenType.GreaterThanEqual] = RELATIONAL_TEST,

                [TokenType.Equivalent] = EQUALITY_TEST,
                [TokenType.Equals] = EQUALITY_TEST,
                [TokenType.NotEquivalent] = EQUALITY_TEST,
                [TokenType.NotEquals] = EQUALITY_TEST,

                [TokenType.BitwiseAnd] = Bitwise_AND,
                [TokenType.BitwiseOr] = Bitwise_OR,

                [TokenType.ConditionalAnd] = CONDITIONAL_AND,
                [TokenType.ConditionalXOr] = CONDITIONAL_XOR,
                [TokenType.ConditionalOr] = CONDITIONAL_OR,

                [TokenType.NullableCoalesce] = NULLABLE_COALESCE,

                [TokenType.TerniaryOperatorTrue] = CONDITIONAL_OPERATOR,
                [TokenType.TerniaryOperatorFalse] = CONDITIONAL_OPERATOR,


                [TokenType.Assignment] = ASSIGNMENT_DECLARATION,
                [TokenType.NullableCoalesceAssign] = ASSIGNMENT_DECLARATION,
                [TokenType.MultiplyAssign] = ASSIGNMENT_DECLARATION,
                [TokenType.ModuloAssign] = ASSIGNMENT_DECLARATION, // todo: bring back modulo assign support ?
                [TokenType.DivideAssign] = ASSIGNMENT_DECLARATION,
                [TokenType.AddAssign] = ASSIGNMENT_DECLARATION,
                [TokenType.SubtractAssign] = ASSIGNMENT_DECLARATION,
                [TokenType.LogicalAndAssign] = ASSIGNMENT_DECLARATION,
                [TokenType.LogicalXOrAssign] = ASSIGNMENT_DECLARATION,
                [TokenType.LogicalOrAssign] = ASSIGNMENT_DECLARATION,
                [TokenType.BitShiftLeftAssign] = ASSIGNMENT_DECLARATION,
                [TokenType.BitShiftRightAssign] = ASSIGNMENT_DECLARATION,
                //[TokenType.Lambda] = ASSIGNMENT_DECLARATION, //todo: support lambdas?

            };

            internal static bool Get(TokenType tokenType, out int precedence)
            {
                var hasPrec = _precendences.TryGetValue(tokenType, out precedence);
                if (!hasPrec)
                {
                    precedence = DEFAULT_OPERATOR_PRECEDENCE - 1; // everything thats unmapped should be lower than default.
                }

                return hasPrec;
            }
        }

        private static class OperatorMetadataFactory
        {
           

            private static readonly Dictionary<TokenType, TokenType> CompountAssignmentTokenTypes = new() // perhaps tokentype should be flags?
            {
                [TokenType.AddAssign] = TokenType.Add,
                [TokenType.SubtractAssign] = TokenType.Subtract,
                [TokenType.DivideAssign] = TokenType.Divide,
                [TokenType.ModuloAssign] = TokenType.Modulo,
                [TokenType.MultiplyAssign] = TokenType.Multiply,
            };

          

            public static OperatorMetadata? Create(TokenType tokenType)
            {
                var isOperator = OperatorPrecedence.Get(tokenType, out var prec);
                if (!isOperator)
                {
                    return null;
                }
                CompountAssignmentTokenTypes.TryGetValue(tokenType, out var decompoundedToken);
                return new OperatorMetadata
                {
                    DecompoundedToken = decompoundedToken,
                    IsCompoundAssignment = CompountAssignmentTokenTypes.ContainsKey(tokenType),
                    IsLeftAssociated = IsLeftAssociated(tokenType),
                    Precedence = prec
                };
            }

            private static bool IsLeftAssociated(TokenType tokenType)
            {
                //return tokenType is not TokenType.Power;
                //todo: refactor for readability and speed...
                // but cant be based on precedence as that is nasty and error prone..
                return tokenType is not (TokenType.NullableCoalesce or TokenType.NullableCoalesceAssign or TokenType.TerniaryOperatorTrue or TokenType.TerniaryOperatorFalse or
                    TokenType.Assignment or TokenType.AddAssign or TokenType.SubtractAssign or TokenType.MultiplyAssign or TokenType.DivideAssign or TokenType.ModuloAssign or TokenType.BitShiftLeftAssign or TokenType.BitShiftRightAssign
                    or TokenType.LogicalAndAssign or TokenType.LogicalOrAssign or TokenType.LogicalXOrAssign);
            }

        }


    }
}
