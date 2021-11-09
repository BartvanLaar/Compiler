
namespace Lexing
{
    public interface ILexer
    {
        Token ConsumeToken();
        Token PeekToken();
        Token[] ConsumeTokens(int amount);
        Token[] PeekTokens(int amount);
    }

    public class Lexer : ILexer
    {
        /// Represents a single file.
        private readonly string _text;
        private int _cursorPosition;
        private int _lineCounter;
        private int _columnCounter;

        public Lexer(string text)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _cursorPosition = 0;
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

            var (tokens, cursor, lineCounter, columnCounter) = TraverseTokensInternal(amount, _cursorPosition, _lineCounter, _columnCounter);

            if (shouldConsume)
            {
                _cursorPosition = cursor;
                _lineCounter = lineCounter;
                _columnCounter = columnCounter;
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

        private (Token Token, int Cursor, int LineCount, int ColumnCount) GetNextToken(int cursor, int lineCount, int columnCount)
        {
            // todo: should this language be case sensitive?
            //todo: should this language except weird characters like latin or arabic (hint: probably only as a char or string value...)?
            (cursor, lineCount, columnCount) = SkipWhiteSpaces(cursor, lineCount, columnCount);

            var columnCountStart = columnCount;
            var token = GetSingleCharacterToken(cursor, lineCount, columnCount);
            if (token is not null)
            {
                cursor++;
                columnCount++;
                (var tokenExtended, cursor, lineCount, columnCount) = GetMultipleCharacterToken(token, cursor, lineCount, columnCount);

                // we don't (yet) care about comments or summaries.. so get rid of them
                if (tokenExtended?.TokenType is (TokenType.Comment or TokenType.Summary))
                {
                    (cursor, lineCount, columnCount) = SkipTillNewLine(cursor, lineCount);
                }

                token = tokenExtended ?? token;
                return (token, cursor, lineCount, columnCount);
            }

            (token, cursor, lineCount, columnCount) = GetNumberToken(cursor, lineCount, columnCount);
            if (token is not null)
            {
                return (token, cursor, lineCount, columnCount);
            }

            var res = string.Empty;
            // todo: is there an easier way than using GetSingleCharToken every time..?
            while (cursor < _text.Length && !char.IsWhiteSpace(_text[cursor]) && GetSingleCharacterToken(cursor, columnCount, lineCount) is null)
            {
                res += _text[cursor];
                columnCount++;
                cursor++;
            }

            if (LexerConstants.IsPredefinedKeyword(res, out var tokenType))
            {
                var predefinedToken = new Token(tokenType, res, lineCount, columnCountStart) { Value = res };
                predefinedToken.TypeIndicator = LexerConstants.ConvertKeywordToTypeIndicator(res);
                if (res is LexerConstants.Keywords.TRUE or LexerConstants.Keywords.FALSE)
                {
                    predefinedToken.Value = res is LexerConstants.Keywords.TRUE;
                }

                return (predefinedToken, cursor, lineCount, columnCount);
            }
            var tok = new Token(TokenType.Identifier, res, lineCount, columnCountStart) { Value = res };
            // kind of a hack, but check if next single char tok is a parantheseOpen, indicating a function call or definition
            // but first eat all white spaces.
            (cursor, lineCount, columnCount) = SkipWhiteSpaces(cursor, lineCount, columnCount);

            var nextTok = GetSingleCharacterToken(cursor, lineCount, columnCountStart);
            if (nextTok?.TokenType == TokenType.ParanthesesOpen)
            {
                // We wont increase any counts. This will lead to duplicate detection of a single character.. being (... But for now, we will take that performance 'hit'.
                tok.TokenType = TokenType.FunctionName;
            }

            return (tok, cursor, lineCount, columnCount);
        }

        private (Token? Token, int Cursor, int LineCount, int ColumnCount) GetNumberToken(int cursor, int lineCount, int columnCount)
        {
            if (!char.IsDigit(_text[cursor]))
            {
                return (null, cursor, lineCount, columnCount);
            }

            var currentChar = _text[cursor];
            var isHexadecimal = cursor + 1 < _text.Length && IsHexIndicator(_text[cursor], _text[cursor + 1]);
            Func<char, bool> isValidCharacter = isHexadecimal
                ? c => char.IsLetterOrDigit(c)
                : c => char.IsDigit(c) || IsDecimalSeparator(c) || IsNumberIndentation(c) || global::Lexing.Lexer.IsNumberIndicator(c);

            var originalColumnCount = columnCount;
            var result = string.Empty;
            while (cursor < _text.Length && isValidCharacter(_text[cursor]))
            {
                result += _text[cursor];
                cursor++;
                columnCount++;
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
                case TypeIndicator.Integer: token.Value = int.Parse(result); break;
                case TypeIndicator.Double: token.Value = double.Parse(result); break;
                case TypeIndicator.Float: token.Value = float.Parse(result); break;
            }

            return (token, cursor, lineCount, columnCount);
        }

        private (Token? Token, int Cursor, int LineCount, int ColumnCount) GetMultipleCharacterToken(Token token, int cursor, int lineCount, int columnCount)
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
                            token.Value = LexerConstants.EQUIVALENT_SIGN;
                            cursor++;
                            columnCount++;
                        }

                        singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.Equals;
                            token.Value = LexerConstants.EQUALS_SIGN;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.LogicalOr:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.LogicalOr)
                        {
                            token.TokenType = TokenType.ConditionalOr;
                            token.Value = LexerConstants.OR_ELSE;
                            cursor++;
                            columnCount++;
                        }
                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.LogicalAnd:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.LogicalAnd)
                        {
                            token.TokenType = TokenType.ConditionalAnd;
                            token.Value = LexerConstants.AND_ALSO;
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
                            token.Value = LexerConstants.NOT_EQUIVALENT_SIGN;
                            cursor++;
                            columnCount++;
                        }

                        singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.NotEquals;
                            token.Value = LexerConstants.NOT_EQUALS_SIGN;
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
                            token.Value = LexerConstants.NULLABLE_COALESCE;
                            cursor++;
                            columnCount++;
                        }

                        singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.NullableCoalesceAssign;
                            token.Value = LexerConstants.NULLABLE_COALESCE_ASSIGN;
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
                            token.Value = LexerConstants.LESS_THAN_EQUAL_SIGN;
                            cursor++;
                            columnCount++;
                        }

                        if (singleCharTok?.TokenType == TokenType.LessThan)
                        {
                            token.TokenType = TokenType.BitShiftLeft;
                            token.Value = LexerConstants.BIT_SHIFT_LEFT;
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
                            token.Value = LexerConstants.GREATER_THAN_EQUAL_SIGN;
                            cursor++;
                            columnCount++;
                        }

                        if (singleCharTok?.TokenType == TokenType.GreaterThan)
                        {
                            token.TokenType = TokenType.BitShiftRight;
                            token.Value = LexerConstants.BIT_SHIFT_RIGHT;
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
                            token.Value = LexerConstants.PLUS_ASSIGN;
                            cursor++;
                            columnCount++;
                        }
                        else if (singleCharTok?.TokenType == TokenType.Add)
                        {
                            token.TokenType = TokenType.AddAdd;
                            token.Value = LexerConstants.PLUS_PLUS;
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
                            token.Value = LexerConstants.MINUS_ASSIGN;
                            cursor++;
                            columnCount++;
                        }
                        else if (singleCharTok?.TokenType == TokenType.GreaterThan)
                        {
                            token.TokenType = TokenType.ReturnTypeIndicator;
                            token.Value = LexerConstants.RETURN_TYPE_INDICATOR;
                            cursor++;
                            columnCount++;
                        }
                        else if (singleCharTok?.TokenType == TokenType.Subtract)
                        {
                            token.TokenType = TokenType.SubtractSubtract;
                            token.Value = LexerConstants.MINUS_MINUS;
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
                            token.Value = LexerConstants.TIMES_ASSIGN;
                            cursor++;
                            columnCount++;
                        }

                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.Modulo:
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        if (singleCharTok?.TokenType == TokenType.Assignment)
                        {
                            token.TokenType = TokenType.ModuloAssign;
                            token.Value = LexerConstants.MODULO_ASSIGN;
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
                            token.Value = LexerConstants.DIVIDE_ASSIGN;
                            cursor++;
                            columnCount++;
                        }

                        if (singleCharTok?.TokenType == TokenType.Divide)
                        {
                            token.TokenType = TokenType.Comment;
                            token.Value = LexerConstants.COMMENT_INDICATOR;
                            cursor++;
                            columnCount++;

                            singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                            if (singleCharTok?.TokenType == TokenType.Divide)
                            {
                                token.TokenType = TokenType.Summary;
                                token.Value = LexerConstants.SUMMARY_INDICATOR;
                                cursor++;
                                columnCount++;
                            }

                        }
                        return (token, cursor, lineCount, columnCount);
                    }
                case TokenType.Value when (token.TypeIndicator is TypeIndicator.String):
                    {
                        var singleCharTok = GetSingleCharacterToken(cursor, lineCount, columnCount);
                        var result = string.Empty;
                        while (cursor < _text.Length)
                        {
                            try
                            {

                                if (singleCharTok is null)
                                {
                                    //todo: handle newlines?
                                    result += _text[cursor];
                                    continue;
                                }

                                if (singleCharTok.TokenType == TokenType.EndOfFile)
                                {
                                    // @todo @cleanup should probably throw an exception?
                                    break;
                                }

                                if (singleCharTok.TypeIndicator == TypeIndicator.String)
                                {
                                    if (cursor + 1 < _text.Length && _text[cursor + 1].ToString() == LexerConstants.CHARACTER_INDICATOR)
                                    {
                                        token.TypeIndicator = TypeIndicator.Character;
                                        cursor++;
                                        columnCount++;
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
                        token.Value = result;
                        return (token, cursor, lineCount, columnCount);
                    }
                default: return (null, cursor, lineCount, columnCount);
            }
        }

        private Token? GetSingleCharacterToken(int cursor, int lineCount, int columnCount)
        {
            if (cursor >= _text.Length)
            {
                return new Token(TokenType.EndOfFile, lineCount, columnCount);
            }

            //@incomplete
            // todo: Why not make this a static dictionary in LexerConstants as this is accessed a **** lot... and then set line + columnCount afterwards...
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
                LexerConstants.AND => new Token(TokenType.LogicalAnd, lineCount, columnCount) { Value = LexerConstants.AND },
                LexerConstants.OR => new Token(TokenType.LogicalOr, lineCount, columnCount) { Value = LexerConstants.OR },
                _ => null,
            };
        }

        private (int cursor, int lineCount, int columnCount) SkipTillNewLine(int cursor, int lineCount)
        {
            while (cursor < _text.Length && _text[cursor] != '\n')
            {
                cursor++;
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

        private static bool IsNumberIndicator(char currentChar)
        {
            return currentChar == LexerConstants.FLOAT_INDICATOR ||
                    currentChar == LexerConstants.DOUBLE_INDICATOR;
        }

    }
}
