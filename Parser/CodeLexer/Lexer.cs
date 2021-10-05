
namespace Parser.CodeLexer
{
    internal interface ILexer
    {
        Token ConsumeToken();
        Token PeekToken();
        Token[] ConsumeTokens(int amount);
        Token[] PeekTokens(int amount);
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
            // todo: should this language be case sensitive?
            //todo: should this language except weird characters like latin or arabic (hint: probably only as a char or string value...)?
            (cursor, lineCount, columnCount) = SkipWhiteSpaces(cursor, lineCount, columnCount);

            var columnCountStart = columnCount;
            var token = GetSingleCharacterToken(cursor, lineCount, columnCount);
            if (token.HasValue)
            {
                cursor++;
                columnCount++;
                (var tokenExtended, cursor, lineCount, columnCount) = GetMultipleCharacterToken(token.Value, cursor, lineCount, columnCount);

                // we don't (yet) care about comments or summaries.. so get rid of them
                if (tokenExtended?.TokenType is (TokenType.Comment or TokenType.Summary))
                {
                    (cursor, lineCount, columnCount) = SkipTillNewLine(cursor, lineCount);
                }

                token = tokenExtended ?? token;
                return (token.Value, cursor, lineCount, columnCount);
            }

            (token, cursor, lineCount, columnCount) = GetNumberToken(cursor, lineCount, columnCount);
            if (token.HasValue)
            {
                return (token.Value, cursor, lineCount, columnCount);
            }

                        
            
            var res = string.Empty;
            while (cursor < _text.Length && !char.IsWhiteSpace(_text[cursor]) && !GetSingleCharacterToken(cursor, columnCount, lineCount).HasValue)
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

            // for now assume it's an identifier...
            // todo: fix assumption and check whether its a functionname etc? it's easy to check and might save headaches in the parser.. who knows..
            return (new Token(TokenType.Identifier, res, lineCount, columnCountStart), cursor, lineCount, columnCount);
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
            Func<char, bool> isValidCharacter = isHexadecimal ? c => char.IsLetterOrDigit(c) : c => char.IsDigit(c) || IsDecimalSeparator(c) || IsNumberIndentation(c) || IsNumberIndicator(c);

            var originalColumnCount = columnCount;
            var result = string.Empty;
            while (cursor < _text.Length && isValidCharacter(_text[cursor]))
            {
                result += _text[cursor];
                cursor++;
                columnCount++;
            }

            TokenType tokenType;

            var numberIndicator = result.Last();
            if (IsNumberIndicator(numberIndicator))
            {
                result = result.Substring(0, result.Length - 1);
                tokenType = numberIndicator == LexerConstants.FLOAT_INDICATOR ? TokenType.Float : numberIndicator == LexerConstants.DOUBLE_INDICATOR ? TokenType.Double : TokenType.Integer;
            }
            else
            {
                // also support double? idk why
                tokenType = isHexadecimal
                    ? TokenType.Hexadecimal : result.Contains(LexerConstants.DECIMAL_SEPARATOR_SIGN)
                          ? TokenType.Float : TokenType.Integer;
            }

            var token = new Token(tokenType, lineCount, originalColumnCount);

            switch (tokenType)
            {
                case TokenType.Integer: token.IntegerValue = int.Parse(result); break;
                case TokenType.Hexadecimal:
                case TokenType.Double:
                case TokenType.Float: token.FloatValue = float.Parse(result); break;
            }

            return (token, cursor, lineCount, columnCount);
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
                case TokenType.Add:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.AddAssign;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.Subtract:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.SubtractAssign;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.Multiply:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.MultiplyAssign;
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
                            token.TokenType = TokenType.DivideAssign;
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
                            token.StringValue = LexerConstants.MODULO_ASSIGN;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.String:
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

                                if (singleCharTok.Value.TokenType == TokenType.String)
                                {
                                    if (cursor + 1 < _text.Length && _text[cursor + 1].ToString() == LexerConstants.CHARACTER_INDICATOR)
                                    {
                                        token.TokenType = TokenType.Character;
                                        cursor++;
                                        columnCount++;
                                    }
                                    else
                                    {
                                        token.TokenType = TokenType.String;
                                    }

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
                LexerConstants.END_OF_STATEMENT => new Token(TokenType.EndOfStatement, lineCount, columnCount) { StringValue = LexerConstants.END_OF_STATEMENT },
                LexerConstants.ACCOLADES_OPEN => new Token(TokenType.AccoladesOpen, lineCount, columnCount) { StringValue = LexerConstants.ACCOLADES_OPEN },
                LexerConstants.PARANTHESES_OPEN => new Token(TokenType.ParanthesesOpen, lineCount, columnCount) { StringValue = LexerConstants.PARANTHESES_OPEN },
                LexerConstants.PARANTHESES_CLOSE => new Token(TokenType.ParanthesesClose, lineCount, columnCount) { StringValue = LexerConstants.PARANTHESES_CLOSE },
                LexerConstants.ACCOLADES_CLOSE => new Token(TokenType.AccoladesClose, lineCount, columnCount) { StringValue = LexerConstants.ACCOLADES_CLOSE },
                LexerConstants.TERNIARY_OPERATOR_TRUE => new Token(TokenType.TerniaryOperatorTrue, lineCount, columnCount) { StringValue = LexerConstants.TERNIARY_OPERATOR_TRUE },
                LexerConstants.TERNIARY_OPERATOR_FALSE => new Token(TokenType.TerniaryOperatorFalse, lineCount, columnCount) { StringValue = LexerConstants.TERNIARY_OPERATOR_FALSE },
                LexerConstants.PLUS => new Token(TokenType.Add, lineCount, columnCount) { StringValue = LexerConstants.PLUS },
                LexerConstants.MINUS => new Token(TokenType.Subtract, lineCount, columnCount) { StringValue = LexerConstants.MINUS },
                LexerConstants.TIMES => new Token(TokenType.Multiply, lineCount, columnCount) { StringValue = LexerConstants.TIMES },
                LexerConstants.DIVIDE => new Token(TokenType.Divide, lineCount, columnCount) { StringValue = LexerConstants.DIVIDE },
                LexerConstants.MODULO => new Token(TokenType.Modulo, lineCount, columnCount) { StringValue = LexerConstants.MODULO },
                LexerConstants.ASSIGN_OPERATOR => new Token(TokenType.Assignment, lineCount, columnCount) { StringValue = LexerConstants.ASSIGN_OPERATOR },
                LexerConstants.NOT_SIGN => new Token(TokenType.BooleanInvert, lineCount, columnCount) { StringValue = LexerConstants.NOT_SIGN },
                LexerConstants.GREATER_THAN_SIGN => new Token(TokenType.GreaterThan, lineCount, columnCount) { StringValue = LexerConstants.GREATER_THAN_SIGN },
                LexerConstants.LESS_THAN_SIGN => new Token(TokenType.LessThan, lineCount, columnCount) { StringValue = LexerConstants.LESS_THAN_SIGN },
                LexerConstants.DOUBLE_QOUTE => new Token(TokenType.String, lineCount, columnCount) { StringValue = LexerConstants.DOUBLE_QOUTE },
                _ => null,
            };
        }

        private (int cursor, long lineCount, long columnCount) SkipTillNewLine(int cursor, long lineCount)
        {
            while (cursor < _text.Length && _text[cursor] != '\n')
            {
                cursor++;
            }

            return (cursor, lineCount + 1, 0);
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
            return currentChar.ToString() == LexerConstants.DECIMAL_SEPARATOR_SIGN;
        }

        private static bool IsNumberIndentation(char currentChar)
        {
            return currentChar.ToString() == LexerConstants.NUMBER_INDENTATION;
        }

        private bool IsNumberIndicator(char currentChar)
        {
            return currentChar == LexerConstants.FLOAT_INDICATOR ||
                    currentChar == LexerConstants.DOUBLE_INDICATOR;
        }

    }
}
