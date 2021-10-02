namespace Compiler
{
    internal interface ILexer
    {
        Token ConsumeToken();
        Token PeekToken();
        Token[] ConsumeTokens(int amount);
        Token[] PeekTokens(int amount);
    }

    internal enum TokenType
    {
        ToDo = -1337,               // should be removed or repurposed...
        Error = -2,

        EndOfFile = -1,
        Undefined,

        Number,
        Float,
        Double,
        Integer,
        Character,
        String,
        DateTime,
        Hexadecimal,

        AccoladesOpen,              // {
        AccoladesClose,             // }
        BracketsOpen,               // [
        BracketsClose,              // ]
        ParanthesesOpen,            // (
        ParanthesesClose,           // )
        Assignment,                 // =
        Equivalent,                 // ==
        NotEquivalent,              // !=
        Equals,                     // ===
        NotEquals,                  // !==
        GreaterThan,                // >
        GreaterThanOrEqualTo,       // >=
        LessThan,                   // <
        LessThanOrEqualTo,          // <=

        Also,                       // &
        AndAlso,                    // &&
        Or,                         // |
        OrElse,                     // ||
        xOr,                        // ^
        BitShiftLeft,               // <<
        BitShiftRight,              // >>>

        If,
        While,
        Do,
        For,
        ForEach,
        In,

        Function_Name,
        VariableName,
        Variable_Type,
        Letters,
        EndOfStatement,             // ;
        TerniaryOperatorTrue,       // ?
        TerniaryOperatorFalse,      // :
        Plus,                       // +
        Minus,                      // -
        Times,                      // *
        Divide,                     // /
        Modulo,                     // %
        NullableCoalesce,           // ??
        PlusAssignment,             // +=
        MinusAssignment,            // -=
        TimesAssignment,            // *=
        DivideAssignment,           // /=
        ModuloAssignment,           // %=
        NullableCoalesceAssignment, // ??=
        BooleanInvert,              // !
        Summary,                    ///        
        Comment,                    //
        VariableTypeInferred,       // var
        PublicScope,                // public
        PrivateScope,               // private
        InternalScope,              // internal
        ProtectedScope,             // protected

    }

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

    internal struct Token
    {

        public TokenType TokenType { get; internal set; }
        public long Line { get; init; }
        public long Column { get; init; }
        public string Name { get; init; }
        public string? StringValue { get; internal set; }
        public double? FloatValue { get; internal set; }
        public long? IntegerValue { get; internal set; }
        public bool? BooleanValue { get; internal set; }
        public Token(TokenType tokenType, long line, long column) : this(tokenType, tokenType.ToString(), line, column) { }
        public Token(long line, long column) : this(default, ((TokenType)default).ToString(), line, column) { }
        public Token(TokenType tokenType, char name, long line, long column) : this(tokenType, name.ToString(), line, column) { }
        public Token(TokenType tokenType, string name, long line, long column)
        {
            TokenType = tokenType;
            Line = line;
            Column = column;
            Name = name;
            StringValue = null;
            FloatValue = null;
            IntegerValue = null;
            BooleanValue = null;
        }

        public override string? ToString()
        {
            return $"{StringValue ?? FloatValue ?? IntegerValue as object ?? Name ?? TokenType.ToString()} - {TokenType}";
        }
    }

    internal class Lexer : ILexer
    {
        /// Represents a single file.
        private string _text;
        private int _cursor;
        private long _lineCounter;
        private long _columnCounter;
        public Lexer(string text)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _cursor = 0;
            _lineCounter = 1;
            _columnCounter = 0;
        }
        public Token ConsumeToken()
        {
            return TraverseTokens(1, true).First();
        }

        public Token PeekToken()
        //TODO: should we cache peek results?
        {
            return TraverseTokens(1, false).First();
        }

        public Token[] ConsumeTokens(int amount)
        {
            return TraverseTokens(amount, true);
        }

        public Token[] PeekTokens(int amount)
        //TODO: should we cache peek results?
        {
            return TraverseTokens(amount, false);
        }

        private Token[] TraverseTokens(int amount, bool shouldConsume)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("amount must be greater than 0.");
            }

            var (tokens, cursor, lineCounter, columnCounter) = TraverseTokensInternal(amount, _cursor, _lineCounter, _columnCounter);

            if (shouldConsume)
            {
                _cursor = cursor;
                _lineCounter = lineCounter;
                _columnCounter = columnCounter;
            }

            return tokens;
        }

        private (Token[] Tokens, int Cursor, long LineCounter, long ColumnCounter) TraverseTokensInternal(int amount, int cursor, long lineCounter, long columnCounter)
        {
            Token[] tokens = new Token[amount];
            for (var counter = 0; counter < amount; counter++)
            {
                (Token token, cursor, lineCounter, columnCounter) =
                    cursor < _text.Length
                    ? GetNextToken(cursor, lineCounter, columnCounter) //@speed :ugly way to fill the rest of the list with values, so the calling side can expect the amount of indexes..
                                                                       //I don't know if this is required, look back at this in the future.
                    : (new Token(TokenType.EndOfFile, lineCounter, columnCounter), cursor, lineCounter, columnCounter);
                tokens[counter] = token;
            }

            return (tokens, cursor, lineCounter, columnCounter);
        }

        private (Token Token, int Cursor, long LineCount, long ColumnCount) GetNextToken(int cursor, long lineCount, long columnCount)
        {
            //todo: should this language except weird characters like latin or arabic (hint: probably only as a char or string value...)?
            (cursor, lineCount, columnCount) = SkipWhiteSpaces(cursor, lineCount, columnCount);

            var columnCountStart = columnCount;
            var token = GetSingleCharacterToken(cursor, lineCount, columnCount);
            if (token.HasValue)
            {
                cursor++;
                columnCount++;
                (var tokenExtended, cursor, lineCount, columnCount) = GetMultipleCharacterToken(token.Value, cursor, lineCount, columnCount);
                token = tokenExtended ?? token;
                return (token.Value, cursor, lineCount, columnCount);
            }

            (token, cursor, lineCount, columnCount) = GetNumberToken(cursor, lineCount, columnCount);
            if (token.HasValue)
            {
                return (token.Value, cursor, lineCount, columnCount);
            }

            //todo: how to determine if token is done? .... Whitespace check wont cut it... as functionname () should also be valid?
            //check against all pre defined keywords..
            // If its not a match, then it's either a variable name, class name, Namespance? etc... or a function name...? 
            // Those edge cases can be checked afterwards, first check all known words.
            // todo: should this language be case sensitive?
            var res = string.Empty;
            while (cursor < _text.Length && !char.IsWhiteSpace(_text[cursor]))
            {
                res += _text[cursor];
                columnCount++;
                cursor++;
            }

            //check against all pre defined keywords..
            //todo: should this lang be case (in)sensitive?
            if (LexerConstants.PredefinedKeyWords.TryGetValue(res, out var tokenType))
            {
                //todo: probably do something when encountering certain tokens?
                return (new Token(tokenType, res, lineCount, columnCountStart), cursor, lineCount, columnCount);
            }

            // for now assume it's a variable name...
            // todo: fix assumption
            return (new Token(TokenType.VariableName, res, lineCount, columnCountStart), cursor, lineCount, columnCount);
        }

        private (Token? Token, int Cursor, long LineCount, long ColumnCount) GetNumberToken(int cursor, long lineCount, long columnCount)
        {
            //todo: implement... don't forget hexadecimals!
            //todo: support things like 20f, 20d
            if (!char.IsDigit(_text[cursor]))
            {
                return (null, cursor, lineCount, columnCount);
            }
            var currentChar = _text[cursor];
            var isHexadecimal = cursor + 1 < _text.Length && IsHexIndicator(_text[cursor], _text[cursor + 1]);
            Func<char, bool> isValidCharacter = isHexadecimal ? c => char.IsLetterOrDigit(c) : c => char.IsDigit(c) || IsDecimalSeparator(c);

            var originalColumnCount = columnCount;
            var result = string.Empty;
            while (cursor < _text.Length && isValidCharacter(_text[cursor]))
            {
                result += _text[cursor];
                cursor++;
                columnCount++;
            }

            // also support double? idk why
            var tokenType = isHexadecimal
                ? TokenType.Hexadecimal : result.Contains(LexerConstants.DECIMAL_SEPARATOR_SIGN)
                      ? TokenType.Float : TokenType.Integer;

            return (new Token(tokenType, result, lineCount, originalColumnCount), cursor, lineCount, columnCount);
        }

        private (Token? Token, int Cursor, long LineCount, long ColumnCount) GetMultipleCharacterToken(Token token, int cursor, long lineCount, long columnCount)
        {
            //@incomplete
            switch (token.TokenType)
            {
                case TokenType.Assignment:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.Equivalent;
                            cursor++;
                            columnCount++;
                        }

                        singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.Equals;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.BooleanInvert:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.NotEquivalent;
                            cursor++;
                            columnCount++;
                        }

                        singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.NotEquals;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.TerniaryOperatorTrue:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.TerniaryOperatorTrue)
                        {
                            token.TokenType = TokenType.NullableCoalesce;
                            cursor++;
                            columnCount++;
                        }

                        singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.NullableCoalesceAssignment;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.LessThan:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.LessThanOrEqualTo;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.GreaterThan:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.GreaterThanOrEqualTo;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.Plus:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.PlusAssignment;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.Minus:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.MinusAssignment;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.Times:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.TimesAssignment;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.Divide:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.DivideAssignment;
                            cursor++;
                            columnCount++;
                        }

                        if (singleCharTok?.TokenType == TokenType.Divide)
                        {
                            cursor++;
                            columnCount++;
                            token.TokenType = TokenType.Comment;

                            singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                            if (singleCharTok?.TokenType == TokenType.Divide)
                            {
                                cursor++;
                                columnCount++;
                                token.TokenType = TokenType.Summary;
                            }

                        }
                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.Modulo:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.ModuloAssignment;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.Character:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        var result = string.Empty;
                        while (cursor < _text.Length)
                        {
                            try
                            {

                                if (!singleCharTok.HasValue)
                                {
                                    //todo: handle newlines?
                                    result += _text[cursor];
                                    continue;
                                }

                                if (singleCharTok.Value.TokenType == TokenType.EndOfFile)
                                {
                                    break;
                                }

                                if (singleCharTok.Value.TokenType == TokenType.Character)
                                {
                                    token.TokenType = TokenType.Character;
                                    break;
                                }
                            }
                            finally
                            {
                                cursor++;
                                columnCount++;
                                singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                            }
                        }
                        token.StringValue = result;
                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.String:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        var result = String.Empty;
                        while (cursor < _text.Length)
                        {
                            try
                            {

                                if (!singleCharTok.HasValue)
                                {
                                    //todo: handle newlines?
                                    result += _text[cursor];
                                    continue;
                                }

                                if (singleCharTok.Value.TokenType == TokenType.EndOfFile)
                                {
                                    break;
                                }

                                if (singleCharTok.Value.TokenType == TokenType.String)
                                {
                                    token.TokenType = TokenType.Character;
                                    break;
                                }
                            }
                            finally
                            {
                                cursor++;
                                columnCount++;
                                singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                            }
                        }
                        token.StringValue = result;
                        return (token, cursor, lineCount, columnCount);
                    }
                default: return (null, cursor, lineCount, columnCount);
            }
        }

        private Token? GetSingleCharacterToken(int cursor, long lineCount, long columnCount)
        {
            if (cursor >= _text.Length)
            {
                return new Token(TokenType.EndOfFile, lineCount, columnCount);
            }

            //@incomplete
            return _text[cursor].ToString() switch
            {
                LexerConstants.END_OF_STATEMENT => new Token(TokenType.EndOfStatement, lineCount, columnCount),
                LexerConstants.ACCOLADES_OPEN => new Token(TokenType.AccoladesOpen, lineCount, columnCount),
                LexerConstants.ACCOLADES_CLOSE => new Token(TokenType.AccoladesClose, lineCount, columnCount),
                LexerConstants.TERNIARY_OPERATOR_TRUE => new Token(TokenType.TerniaryOperatorTrue, lineCount, columnCount),
                LexerConstants.TERNIARY_OPERATOR_FALSE => new Token(TokenType.TerniaryOperatorFalse, lineCount, columnCount),
                LexerConstants.PLUS_SIGN => new Token(TokenType.Plus, lineCount, columnCount),
                LexerConstants.MINUS_SIGN => new Token(TokenType.Minus, lineCount, columnCount),
                LexerConstants.TIMES_SIGN => new Token(TokenType.Times, lineCount, columnCount),
                LexerConstants.DIVIDE_SIGN => new Token(TokenType.Divide, lineCount, columnCount),
                LexerConstants.MODULO_SIGN => new Token(TokenType.Modulo, lineCount, columnCount),
                LexerConstants.ASSIGN_OPERATOR => new Token(TokenType.Assignment, lineCount, columnCount),
                LexerConstants.NOT_SIGN => new Token(TokenType.BooleanInvert, lineCount, columnCount),
                LexerConstants.GREATER_THAN_SIGN => new Token(TokenType.GreaterThan, lineCount, columnCount),
                LexerConstants.LESS_THAN_SIGN => new Token(TokenType.LessThan, lineCount, columnCount),
                LexerConstants.SINGLE_QOUTE => new Token(TokenType.Character, lineCount, columnCount),
                LexerConstants.DOUBLE_QOUTE => new Token(TokenType.String, lineCount, columnCount),
                _ => null,
            };
        }

        private (int cursor, long lineCount, long columnCount) SkipWhiteSpaces(int cursor, long lineCount, long columnCount)
        {
            while (cursor < _text.Length && char.IsWhiteSpace(_text[cursor]))
            {
                if (_text[cursor] == '\n')
                {
                    columnCount = 1;
                    lineCount++;
                }
                else
                {
                    columnCount++;
                }

                cursor++;
            }

            return (cursor, lineCount, columnCount);
        }

        private static bool IsHexIndicator(char currentChar, char nextChar)
        {
            return LexerConstants.HEX_SIGN[0] == currentChar && LexerConstants.HEX_SIGN[1] == nextChar;
        }

        private static bool IsDecimalSeparator(char currentChar)
        {
            return currentChar == LexerConstants.DECIMAL_SEPARATOR_SIGN;
        }

        private bool IsNumberIndicator(char currentChar)
        {
            return currentChar == LexerConstants.FLOAT_INDICATOR ||
                    currentChar == LexerConstants.DOUBLE_INDICATOR;
        }

    }
}
