
using Exceptions;

namespace Lexing
{
    public interface ILexer
    {
        Token ConsumeToken();
        Token PeekToken();
        Token[] ConsumeTokens(int amount);
        Token[] PeekTokens(int amount);
        Token[] GetConsumedTokens(int amount);
        Token? GetLastConsumedToken();
    }

    public class Lexer : ILexer
    {
        const int MAX_DESCOVERED_TOKEN_CACHE = 10;
        /// Represents a single file.
        private const string DIRECT_TEXT_INPUT = "Direct input";
        private readonly string _text;
        private readonly string _filename;
        private int _cursorPosition;
        private int _lineCounter;
        private int _columnCounter;

        private List<Token> _traversedTokens = new List<Token>();
        private List<Token> _consumedTokens = new List<Token>();

        public Lexer(string text) : this(text, DIRECT_TEXT_INPUT) { }
        public Lexer(string text, string filename)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _filename = filename;
            _cursorPosition = 0;
            _lineCounter = 1;
            _columnCounter = 0;
        }

        public Token ConsumeToken()
        {
            return TraverseTokens(1, true).First();
        }

        //TODO: should we cache peek results?
        public Token PeekToken()
        {
            return TraverseTokens(1, false).First();
        }

        public Token[] ConsumeTokens(int amount)
        {
            return TraverseTokens(amount, true);
        }

        //TODO: should we cache peek results?
        public Token[] PeekTokens(int amount)
        {
            return TraverseTokens(amount, false);
        }

        private Token[] TraverseTokens(int amount, bool shouldConsume)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("amount must be greater than 0.");
            }

            var (tokens, cursor, lineCounter, columnCounter) = TraverseTokensInternal(amount, _cursorPosition, _lineCounter, _columnCounter);

            if (shouldConsume)
            {
                _cursorPosition = cursor;
                _lineCounter = lineCounter;
                _columnCounter = columnCounter;
                _consumedTokens.AddRange(tokens);
            }

            return tokens;
        }

        private (Token[] Tokens, int Cursor, int LineCounter, int ColumnCounter) TraverseTokensInternal(int amount, int cursor, int lineCounter, int columnCounter)
        {
            Token[] tokens = new Token[amount];
            // @FutureMe, if doing this for a single token peek is a performance bottleneck, you're doing some crazy fast stuff... and you're allowed to refactor this for the PeekToken() and ConsumeToken() methods :), for now idc.... ~Bart, 07-10-2021
            for (var counter = 0; counter < amount; counter++)
            {
                (Token token, cursor, lineCounter, columnCounter) =
                    cursor < _text.Length
                    ? GetNextToken(cursor, lineCounter, columnCounter)
                    : (new Token(TokenType.EndOfFile, lineCounter, columnCounter), cursor, lineCounter, columnCounter);
                tokens[counter] = token;
            }

            return (tokens, cursor, lineCounter, columnCounter);
        }

        private Token? GetlastTraversedToken()
        {
            return _traversedTokens.LastOrDefault();
        }

        private void AddDiscoveredTokenToCache(Token token)
        {
            if (_traversedTokens.Count > MAX_DESCOVERED_TOKEN_CACHE)
            {
                _traversedTokens.RemoveAt(0);
            }
            _traversedTokens.Add(token);
        }

        private (Token Token, int Cursor, int LineCount, int ColumnCount) GetNextToken(int currentCursor, int currentLineCount, int currentColumnCount)
        {
            //todo: should this language except weird characters like latin or arabic (hint: probably only as a char or string value...)?
            (currentCursor, currentLineCount, currentColumnCount) = SkipWhiteSpaces(currentCursor, currentLineCount, currentColumnCount);

            (var symbolToken, currentCursor, currentLineCount, currentColumnCount) = GetNextSymbolToken(currentCursor, currentLineCount, currentColumnCount);
            if (symbolToken is not null)
            {
                AddDiscoveredTokenToCache(symbolToken);
                return (symbolToken, currentCursor, currentLineCount, currentColumnCount);
            }

            (var numberToken, currentCursor, currentLineCount, currentColumnCount) = GetNumberToken(currentCursor, currentLineCount, currentColumnCount);
            if (numberToken is not null)
            {
                AddDiscoveredTokenToCache(numberToken);
                return (numberToken, currentCursor, currentLineCount, currentColumnCount);
            }

            // save start column count for retrieving identifier.
            var columnCountStart = currentColumnCount;
            // Consume identifier...
            (var identifier, currentCursor, currentLineCount, currentColumnCount) = GetIdentifier(currentCursor, currentLineCount, currentColumnCount);
            if (LexerConstants.IsPredefinedKeyword(identifier, out var tokenType) && GetlastTraversedToken()?.TokenType is TokenType.Dot)
            {
                var predefinedToken = new Token(tokenType, identifier, currentLineCount, columnCountStart) { Value = identifier };
                predefinedToken.TypeIndicator = LexerConstants.ConvertKeywordToTypeIndicator(identifier);
                if (identifier is LexerConstants.Keywords.TRUE or LexerConstants.Keywords.FALSE)
                {
                    predefinedToken.Value = identifier is LexerConstants.Keywords.TRUE;
                }

                return (predefinedToken, currentCursor, currentLineCount, currentColumnCount);
            }
            var tok = new Token(TokenType.Identifier, identifier, currentLineCount, columnCountStart) { Value = identifier };
            // kind of a hack, but check if next single char tok is a parantheseOpen, indicating a function call or definition
            // but first eat all white spaces.
            (currentCursor, currentLineCount, currentColumnCount) = SkipWhiteSpaces(currentCursor, currentLineCount, currentColumnCount);

            var nextTok = GetSingleCharacterToken(currentCursor, currentLineCount, currentColumnCount);
            if (nextTok?.TokenType == TokenType.ParanthesesOpen)
            {
                // We wont increase any counts.
                // as this token is important to the parser for detecting func args and syntax check.                                
                tok.TokenType = TokenType.Identifier;
                AddDiscoveredTokenToCache(tok);

                return (tok, currentCursor, currentLineCount, currentColumnCount);
            }
            AddDiscoveredTokenToCache(tok);
            return (tok, currentCursor, currentLineCount, currentColumnCount);
        }

        private (string Identifier, int Cursor, int LineCount, int ColumnCount) GetIdentifier(int cursor, int lineCount, int columnCount)
        {
            var identifier = string.Empty;
            // todo: is there an easier way than using GetSingleCharToken every time..?
            while (cursor < _text.Length && !char.IsWhiteSpace(_text[cursor]) && GetSingleCharacterToken(cursor, columnCount, lineCount) is null)
            {
                identifier += _text[cursor];
                ++columnCount;
                ++cursor;
            }

            return (identifier, cursor, lineCount, columnCount);
        }

        private (Token? Token, int Cursor, int LineCount, int ColumnCount) GetNumberToken(int cursor, int lineCount, int columnCount, bool isNegative = false)
        {
            if (cursor >= _text.Length || !char.IsDigit(_text[cursor]))
            {
                return (null, cursor, lineCount, columnCount);
            }

            var currentChar = _text[cursor];
            var isHexadecimal = cursor + 1 < _text.Length && IsHexIndicator(_text[cursor], _text[cursor + 1]);
            Func<char, bool> isValidCharacter = isHexadecimal
                ? c => char.IsLetterOrDigit(c)
                : c => char.IsDigit(c) || IsDecimalSeparator(c) || IsNumberIndentation(c) || IsNumberIndicator(c);

            var originalColumnCount = columnCount;
            var result = string.Empty;
            while (cursor < _text.Length && isValidCharacter(_text[cursor]))
            {
                result += _text[cursor];
                ++cursor;
                ++columnCount;
            }

            TypeIndicator typeIndicator;

            var numberIndicator = result.Last();
            if (IsNumberIndicator(numberIndicator))
            {
                result = result[0..^1];
                typeIndicator = numberIndicator == LexerConstants.FLOAT_INDICATOR
                    ? TypeIndicator.Float : numberIndicator == LexerConstants.DOUBLE_INDICATOR
                        ? TypeIndicator.Double : TypeIndicator.Integer;
            }
            else
            {
                // also support float? idk why
                typeIndicator = isHexadecimal
                    ? TypeIndicator.Integer : result.Contains(LexerConstants.DECIMAL_SEPARATOR_SIGN)
                          ? TypeIndicator.Double : TypeIndicator.Integer;
            }

            var token = new Token(TokenType.Value, lineCount, originalColumnCount)
            {
                TypeIndicator = typeIndicator
            };

            switch (token.TypeIndicator)
            {
                case TypeIndicator.Integer: token.Value = long.Parse(result); break;
                case TypeIndicator.Double: token.Value = double.Parse(result); break;
                case TypeIndicator.Float: token.Value = float.Parse(result); break;
            }

            return (token, cursor, lineCount, columnCount);
        }

        private (Token? Token, int Cursor, int LineCount, int ColumnCount) GetNextSymbolToken(int cursor, int lineCount, int columnCount)
        {
            var token = GetSingleCharacterToken(cursor, lineCount, columnCount);
            if (token is not null)
            {
                ++cursor;
                ++columnCount;
                (var tokenExtended, cursor, lineCount, columnCount) = GetMultipleCharacterToken(token, cursor, lineCount, columnCount);
                token = tokenExtended ?? token;

                // we don't (yet) care about comments or summaries.. so get rid of them
                if (token.TokenType is (TokenType.Comment or TokenType.Summary))
                {
                    (cursor, lineCount, columnCount) = SkipTillNewLine(cursor, lineCount);

                    return (token, cursor, lineCount, columnCount);
                }

                return (token, cursor, lineCount, columnCount);
            }
            return (null, cursor, lineCount, columnCount);
        }


        private (Token? Token, int Cursor, int LineCount, int ColumnCount) GetMultipleCharacterToken(Token token, int cursor, int lineCount, int columnCount)
        {
            switch (token.TokenType)
            {
                case TokenType.BracketOpen:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.BracketClose)
                        {
                            token.TokenType = TokenType.Array;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }

                        break;
                    }
                case TokenType.Assignment:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.Equivalent;
                            token.Value = LexerConstants.EQUIVALENT_SIGN;

                            ++cursor;
                            ++columnCount;

                            singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                            if (singleCharTok?.TokenType == TokenType.Assignment)
                            {
                                token.TokenType = TokenType.Equals;
                                token.Value = LexerConstants.EQUALS_SIGN;
                                return (token, ++cursor, lineCount, ++columnCount);
                            }
                            return (token, cursor, lineCount, columnCount);
                        }

                        break;
                    }
                case TokenType.BitwiseOr:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.BitwiseOr)
                        {
                            token.TokenType = TokenType.ConditionalOr;
                            token.Value = LexerConstants.OR_ELSE;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }
                        break;
                    }
                case TokenType.BitwiseAnd:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.BitwiseAnd)
                        {
                            token.TokenType = TokenType.ConditionalAnd;
                            token.Value = LexerConstants.AND_ALSO;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }

                        break;
                    }
                case TokenType.BooleanInvert:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.NotEquivalent;
                            token.Value = LexerConstants.NOT_EQUIVALENT_SIGN;

                            ++cursor;
                            ++columnCount;

                            singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                            if (singleCharTok?.TokenType == TokenType.Assignment)
                            {
                                token.TokenType = TokenType.NotEquals;
                                token.Value = LexerConstants.NOT_EQUALS_SIGN;
                                return (token, ++cursor, lineCount, ++columnCount);

                            }

                            return (token, cursor, lineCount, columnCount);
                        }

                        break;
                    }
                case TokenType.TerniaryOperatorTrue:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.TerniaryOperatorTrue)
                        {
                            token.TokenType = TokenType.NullableCoalesce;
                            token.Value = LexerConstants.NULLABLE_COALESCE;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }

                        singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.NullableCoalesceAssign;
                            token.Value = LexerConstants.NULLABLE_COALESCE_ASSIGN;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }

                        break;
                    }
                case TokenType.LessThan:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.LessThanEqual;
                            token.Value = LexerConstants.LESS_THAN_EQUAL_SIGN;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }

                        if (singleCharTok?.TokenType == TokenType.LessThan)
                        {
                            token.TokenType = TokenType.BitShiftLeft;
                            token.Value = LexerConstants.BIT_SHIFT_LEFT;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }
                        break;
                    }
                case TokenType.GreaterThan:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.GreaterThanEqual;
                            token.Value = LexerConstants.GREATER_THAN_EQUAL_SIGN;
                            return (token, ++cursor, lineCount, ++columnCount);

                        }

                        if (singleCharTok?.TokenType == TokenType.GreaterThan)
                        {
                            token.TokenType = TokenType.BitShiftRight;
                            token.Value = LexerConstants.BIT_SHIFT_RIGHT;
                            return (token, ++cursor, lineCount, ++columnCount);

                        }

                        break;
                    }
                case TokenType.Add:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.AddAssign;
                            token.Value = LexerConstants.PLUS_ASSIGN;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }

                        if (singleCharTok?.TokenType == TokenType.Add)
                        {
                            token.TokenType = TokenType.AddAdd;
                            token.Value = LexerConstants.PLUS_PLUS;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }

                        break;
                    }
                case TokenType.Subtract:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.SubtractAssign;
                            token.Value = LexerConstants.MINUS_ASSIGN;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }

                        if (singleCharTok?.TokenType == TokenType.GreaterThan)
                        {
                            token.TokenType = TokenType.ReturnTypeIndicator;
                            token.Value = LexerConstants.RETURN_TYPE_INDICATOR;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }

                        if (singleCharTok?.TokenType == TokenType.Subtract)
                        {
                            token.TokenType = TokenType.SubtractSubtract;
                            token.Value = LexerConstants.MINUS_MINUS;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }
                        break;

                    }
                case TokenType.Multiply:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.MultiplyAssign;
                            token.Value = LexerConstants.TIMES_ASSIGN;
                            return (token, ++cursor, lineCount, ++columnCount);

                        }

                        break;
                    }
                case TokenType.Modulo:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.ModuloAssign;
                            token.Value = LexerConstants.MODULO_ASSIGN;
                            return (token, ++cursor, lineCount, ++columnCount);

                        }

                        break;
                    }
                case TokenType.Divide:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.DivideAssign;
                            token.Value = LexerConstants.DIVIDE_ASSIGN;
                            return (token, ++cursor, lineCount, ++columnCount);
                        }

                        if (singleCharTok?.TokenType == TokenType.Divide)
                        {
                            token.TokenType = TokenType.Comment;
                            token.Value = LexerConstants.COMMENT_INDICATOR;
                            ++cursor;
                            ++columnCount;

                            singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                            if (singleCharTok?.TokenType == TokenType.Divide)
                            {
                                token.TokenType = TokenType.Summary;
                                token.Value = LexerConstants.SUMMARY_INDICATOR;
                                ++cursor;
                                ++columnCount;
                            }
                            return (token, cursor, lineCount, columnCount);

                        }

                        break;
                    }
                case TokenType.Value when (token.TypeIndicator is TypeIndicator.String):
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        var result = string.Empty;
                        while (cursor < _text.Length)
                        {
                            try
                            {

                                if (singleCharTok?.TokenType is TokenType.EndOfFile)
                                {
                                    throw new SyntaxErrorException(lineCount, cursor, "Unexpected end of file after string indicator.", _filename);
                                }

                                if (singleCharTok?.TypeIndicator is TypeIndicator.String)
                                {
                                    if (cursor + 1 < _text.Length && _text[cursor + 1].ToString() == LexerConstants.CHARACTER_INDICATOR)
                                    {
                                        token.TypeIndicator = TypeIndicator.Character;
                                        ++cursor;
                                        ++columnCount;
                                    }

                                    break;
                                }

                                result += _text[cursor];
                            }
                            finally
                            {
                                ++cursor;
                                ++columnCount;
                                singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                            }
                        }
                        token.Value = result;
                        return (token, cursor, lineCount, columnCount);
                    }
            }
            return (null, cursor, lineCount, columnCount);
        }

        private Token? GetSingleCharacterToken(int cursor, int lineCount, int columnCount)
        {
            if (cursor >= _text.Length)
            {
                return new Token(TokenType.EndOfFile, lineCount, columnCount);
            }

            //@incomplete
            // todo: Why not make this a static dictionary in LexerConstants as this is accessed a lot... and then set line + columnCount afterwards...
            // actually need to know if this is a performance issue though...
            return _text[cursor].ToString() switch
            {
                LexerConstants.END_OF_STATEMENT => new Token(TokenType.EndOfStatement, lineCount, columnCount) { Value = LexerConstants.END_OF_STATEMENT },
                LexerConstants.ARGUMENT_SEPARATOR => new Token(TokenType.ArgumentSeparator, lineCount, columnCount) { Value = LexerConstants.ARGUMENT_SEPARATOR },
                LexerConstants.ACCOLADES_OPEN => new Token(TokenType.AccoladesOpen, lineCount, columnCount) { Value = LexerConstants.ACCOLADES_OPEN },
                LexerConstants.PARANTHESES_OPEN => new Token(TokenType.ParanthesesOpen, lineCount, columnCount) { Value = LexerConstants.PARANTHESES_OPEN },
                LexerConstants.PARANTHESES_CLOSE => new Token(TokenType.ParanthesesClose, lineCount, columnCount) { Value = LexerConstants.PARANTHESES_CLOSE },
                LexerConstants.BACKETS_OPEN => new Token(TokenType.BracketOpen, lineCount, columnCount) { Value = LexerConstants.BACKETS_OPEN },
                LexerConstants.BACKETS_CLOSE => new Token(TokenType.BracketClose, lineCount, columnCount) { Value = LexerConstants.BACKETS_CLOSE },
                LexerConstants.ACCOLADES_CLOSE => new Token(TokenType.AccoladesClose, lineCount, columnCount) { Value = LexerConstants.ACCOLADES_CLOSE },
                LexerConstants.TERNIARY_OPERATOR_TRUE => new Token(TokenType.TerniaryOperatorTrue, lineCount, columnCount) { Value = LexerConstants.TERNIARY_OPERATOR_TRUE },
                LexerConstants.TERNIARY_OPERATOR_FALSE => new Token(TokenType.TerniaryOperatorFalse, lineCount, columnCount) { Value = LexerConstants.TERNIARY_OPERATOR_FALSE },
                LexerConstants.PLUS => new Token(TokenType.Add, lineCount, columnCount) { Value = LexerConstants.PLUS },
                LexerConstants.MINUS => new Token(TokenType.Subtract, lineCount, columnCount) { Value = LexerConstants.MINUS },
                LexerConstants.TIMES => new Token(TokenType.Multiply, lineCount, columnCount) { Value = LexerConstants.TIMES },
                LexerConstants.DIVIDE => new Token(TokenType.Divide, lineCount, columnCount) { Value = LexerConstants.DIVIDE },
                LexerConstants.MODULO => new Token(TokenType.Modulo, lineCount, columnCount) { Value = LexerConstants.MODULO },
                //LexerConstants.POWER => new Token(TokenType.Power, lineCount, columnCount) { StringValue = LexerConstants.POWER },
                LexerConstants.ASSIGN_OPERATOR => new Token(TokenType.Assignment, lineCount, columnCount) { Value = LexerConstants.ASSIGN_OPERATOR },
                LexerConstants.NOT_SIGN => new Token(TokenType.BooleanInvert, lineCount, columnCount) { Value = LexerConstants.NOT_SIGN },
                LexerConstants.GREATER_THAN_SIGN => new Token(TokenType.GreaterThan, lineCount, columnCount) { Value = LexerConstants.GREATER_THAN_SIGN },
                LexerConstants.LESS_THAN_SIGN => new Token(TokenType.LessThan, lineCount, columnCount) { Value = LexerConstants.LESS_THAN_SIGN },
                LexerConstants.DOUBLE_QOUTE => new Token(TokenType.Value, lineCount, columnCount) { Value = LexerConstants.DOUBLE_QOUTE, TypeIndicator = TypeIndicator.String },
                LexerConstants.AND => new Token(TokenType.BitwiseAnd, lineCount, columnCount) { Value = LexerConstants.AND },
                LexerConstants.OR => new Token(TokenType.BitwiseOr, lineCount, columnCount) { Value = LexerConstants.OR },
                LexerConstants.DOT => new Token(TokenType.Dot, lineCount, columnCount) { Value = LexerConstants.DOT },
                _ => null,
            };
        }

        private (int cursor, int lineCount, int columnCount) SkipTillNewLine(int cursor, int lineCount)
        {
            while (cursor < _text.Length && _text[cursor] != '\n')
            {
                ++cursor;
            }

            return (cursor, lineCount + 1, 0);
        }

        private (int cursor, int lineCount, int columnCount) SkipWhiteSpaces(int cursor, int lineCount, int columnCount)
        {
            while (cursor < _text.Length && char.IsWhiteSpace(_text[cursor]))
            {
                if (_text[cursor] == '\n')
                {
                    columnCount = 1;
                    ++lineCount;
                }
                else
                {
                    ++columnCount;
                }

                ++cursor;
            }

            return (cursor, lineCount, columnCount);
        }

        private static bool IsHexIndicator(char currentChar, char nextChar)
        {
            return LexerConstants.HEX_SIGN[0] == currentChar && LexerConstants.HEX_SIGN[1] == nextChar;
        }

        private static bool IsDecimalSeparator(char currentChar)
        {
            return currentChar.ToString() == LexerConstants.DECIMAL_SEPARATOR_SIGN;
        }

        private static bool IsNumberIndentation(char currentChar)
        {
            return currentChar.ToString() == LexerConstants.NUMBER_INDENTATION;
        }

        private static bool IsNumberIndicator(char currentChar)
        {
            return currentChar == LexerConstants.FLOAT_INDICATOR ||
                    currentChar == LexerConstants.DOUBLE_INDICATOR;
        }

        public Token[] GetConsumedTokens(int amount)
        {
            return _consumedTokens.TakeLast(amount).ToArray();
        }

        public Token? GetLastConsumedToken()
        {
            return GetConsumedTokens(1).SingleOrDefault();
        }

    }
}
