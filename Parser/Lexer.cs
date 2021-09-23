using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Compiler
{
    //internal interface ILexerResults
    //{
    //    Token[] Tokens { get; }
    //}
    //internal class LexerResults : ILexerResults
    //{
    //    private List<Token> _tokens = new List<Token>();
    //    public Token[] Tokens => _tokens.ToArray();

    //    internal void AddToken(Token token)
    //    {
    //        _tokens.Add(token);
    //    }
    //}

    internal interface ILexer
    {
        Token ConsumeToken();
        Token PeekToken();
        Token[] ConsumeTokens(int amount);
        Token[] PeekTokens(int amount);
    }

    internal enum TokenType
    {
        Error = -2,

        EndOfFile = -1,
        Undefined,

        Number,
        Character,
        String,
        DateTime,
        Hexadecimal,

        AccoladesOpen, // {
        AccoladesClose, // }
        BracketsOpen, // [
        BracketsClose, // ]
        ParanthesesOpen, // (
        ParanthesesClose, // )

        Equivalent, // ==
        NotEquivalent, // !=
        Equals, // ===
        NotEquals, // !==
        GreaterThan,// >
        GreaterThanOrEquivalentTo, // >=
        GreaterThanOrEqualTo, // >==
        LessThan, // <
        LessThanOrEquivalentTo, // <=
        LessThanOrEqualTo, // <==

        Also, // &
        AndAlso, // &&
        Or, // |
        OrElse, // ||

        If,
        While,
        Do,
        For,
        ForEach,
        In,

        Function_Name,
        Variable_Name,
        Variable_Type,
        Letters,
        Float,
        EndOfStatement,
        ToDo,
        TerniaryOperatorTrue,
        TerniaryOperatorFalse,
    }

    internal static class LexerConstants
    {
        public const char PARANTHESES_OPEN = '(';
        public const char PARANTHESES_CLOSE = ')';
        public const char ACCOLADES_OPEN = '{';
        public const char ACCOLADES_CLOSE = '}';
        public const char BACKETS_OPEN = '[';
        public const char BACKETS_CLOSE = ']';

        public const char END_OF_STATEMENT = ';';

        /// Math signs
        public const string PRECENDENCE_BEGIN_SIGN = "(";
        public const string PRECENDENCE_END_SIGN = ")";
        public const string PLUS_SIGN = "+";
        public const string MINUS_SIGN = "-";
        public const string TIMES_SIGN = "*";
        public const string DIVIDE_SIGN = "/";
        public const string MODULO_SIGN = "%";
        public const string EQUALS_MATH_SIGN = "=";

        public const string GREATER_THAN_SIGN = ">";
        public const string GREATER_THAN_EQUAL_SIGN = ">=";
        public const string LESS_THAN_SIGN = "<";
        public const string LESS_THAN_EQUAL_SIGN = "<=";
        public const string EQUALS_COMPARISON_SIGN = "==";
        public const string NOT_EQUALS_SIGN = "!=";

        public const string BOOLEAN_INVERT = "!";

        public const char TERNIARY_OPERATOR_TRUE = '?';
        public const char TERNIARY_OPERATOR_FALSE = ':';

        public const string HEX_SIGN = "0x";
        public const char DECIMAL_SEPARATOR_SIGN = '.';
        public const string NUMBER_INDENTATION = "_";

        public const string SINGLE_QOUTE = "\'";
        public const string DOUBLE_QOUTE = "\"";

        /// Explicit type indicators
        public const char DOUBLE_INDICATOR = 'd';
        public const char FLOAT_INDICATOR = 'f';
        public const char INTEGER_INDICATOR = 'i';
        public const char CHARACTER_INDICATOR = 'c';
        public const char STRING_INDICATOR = 's';

        public const string COMMENTS_INDICATOR = "///";
    }

    internal struct Token
    {

        public TokenType TokenType { get; init; }
        public long Line { get; init; }
        public long Column { get; init; }
        public string Name { get; init; }
        public string? StringValue { get; init; }
        public double? FloatValue { get; init; }
        public long? IntegerValue { get; init; }
        public bool? BooleanValue { get; init; }
        public Token(TokenType tokenType, long line, long column) : this(tokenType, tokenType.ToString(), line, column) { }

        public Token(TokenType tokenType, string name, long row, long column)
        {
            TokenType = tokenType;
            Line = row;
            Column = column;
            Name = name;
            StringValue = null;
            FloatValue = null;
            IntegerValue = null;
            BooleanValue = null;
        }

        public override string? ToString()
        {
            return $"{StringValue ?? FloatValue ?? IntegerValue as object ?? "Undefined"} - {TokenType}";
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
            (cursor, lineCount, columnCount) = SkipWhiteSpaces(cursor, lineCount, columnCount);

            var res = string.Empty;
            var columnCountStart = columnCount;
            var token = GetSingleCharacterToken(cursor, lineCount, columnCount);
            if (token.HasValue)
            {
                columnCount++;
                cursor++;
                return (token.Value, cursor, lineCount, columnCount);
            }

            //todo: how to determine if token is done? .... Whitespace check wont cut it... as functionname () should also be valid?

            while (cursor < _text.Length && !char.IsWhiteSpace(_text[cursor]))
            {
                res += _text[cursor];
                columnCount++;
                cursor++;

            }

            return (new Token(TokenType.ToDo, res, lineCount, columnCountStart), cursor, lineCount, columnCount);
        }

        private Token? GetSingleCharacterToken(int cursor, long lineCount, long columnCount)
        {
            return _text[cursor] switch
            {
                LexerConstants.END_OF_STATEMENT => new Token(TokenType.EndOfStatement, lineCount, columnCount),
                LexerConstants.ACCOLADES_OPEN => new Token(TokenType.AccoladesOpen, lineCount, columnCount),
                LexerConstants.ACCOLADES_CLOSE => new Token(TokenType.AccoladesClose, lineCount, columnCount),
                // todo: peek if next char is a dot (return null) or another ? sign? (also return false)?
                LexerConstants.TERNIARY_OPERATOR_TRUE => new Token(TokenType.TerniaryOperatorTrue, lineCount, columnCount),
                // todo: determine how our language should look syntactically, is there something that can be after a : thats got another meaning?
                LexerConstants.TERNIARY_OPERATOR_FALSE => new Token(TokenType.TerniaryOperatorFalse, lineCount, columnCount),
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
                    currentChar == LexerConstants.DOUBLE_INDICATOR ||
                    currentChar == LexerConstants.INTEGER_INDICATOR;
        }

    }
}
