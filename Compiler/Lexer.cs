using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Compiler
{
    internal interface ILexerResults
    {
        Token[] Tokens { get; }
    }
    internal class LexerResults : ILexerResults
    {
        private List<Token> _tokens = new List<Token>();
        public Token[] Tokens => _tokens.ToArray();

        internal void AddToken(Token token)
        {
            _tokens.Add(token);
        }
    }

    internal interface ILexer
    {
        Task<ILexerResults> LexAsync(string[] textFilesToLex);
        Task<ILexerResults> LexAsync(string textToLex);
    }

    internal enum TokenType
    {
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
    }

    internal static class LexerConstants
    {
        public const string PARANTHESES_OPEN = "(";
        public const string PARANTHESES_CLOSE = ")";
        public const string ACCOLADES_OPEN = "{";
        public const string ACCOLADES_CLOSE = "}";
        public const string BACKETS_OPEN = "[";
        public const string BACKETS_CLOSE = "]";

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

        public const string TERINARY_OPERATOR_1 = "?";
        public const string TERINARY_OPERATOR_2 = ":";

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
    internal struct TokenDefinition
    {
        public TokenDefinition(TokenType tokenType, string regex)
        {
            TokenType = tokenType;
            Regex = regex;
        }
        public TokenType TokenType { get; }
        public string Regex { get; }

    }
    internal struct Token
    {

        public Token(TokenType tokenType, char value, long row, long column) : this(tokenType, value.ToString(), row, column) { }
        public Token(TokenType tokenType, long row, long column) : this(tokenType, string.Empty, row, column) { }

        public Token(TokenType tokenType, string value, long row, long column)
        {
            TokenType = tokenType;
            Value = value;
            Row = row;
            Column = column;
        }


        public TokenType TokenType { get; }
        public long Row { get; }
        public long Column { get; set; }
        public string Value { get; }

        public override string? ToString()
        {
            return $"{Value} - {TokenType}";
        }
    }

    internal class Lexer : ILexer
    {
        private static IDictionary<TokenType, TokenDefinition> _tokens = new Dictionary<TokenType, TokenDefinition>()
        {
            {TokenType.AccoladesOpen, new TokenDefinition(TokenType.AccoladesOpen, LexerConstants.ACCOLADES_OPEN)},
            {TokenType.AccoladesClose, new TokenDefinition(TokenType.AccoladesClose, LexerConstants.ACCOLADES_CLOSE)},
        };

        public async Task<ILexerResults> LexAsync(string textToLex)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(textToLex);
            writer.Flush();
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return LexInternal(reader);
        }
        public async Task<ILexerResults> LexAsync(string[] textFilesToLex)
        {
            //todo: support multiple files.
            using var stream = File.OpenRead(textFilesToLex[0]);
            using var reader = new StreamReader(stream);

            return LexInternal(reader);
        }

        private ILexerResults LexInternal(StreamReader reader)
        {
            var result = new LexerResults();
            Token lastToken;
            long lineCount = 1;
            long columnCount = 0;

            while (!reader.EndOfStream)
            {
                (lastToken, lineCount, columnCount) = ConsumeNextToken(reader, lineCount, columnCount);
                result.AddToken(lastToken);
            }

            if (!result.Tokens.Any() || result.Tokens.Last().TokenType != TokenType.EndOfFile)
            {
                result.AddToken(new Token(TokenType.EndOfFile, lineCount, columnCount));
            }

            return result;
        }

        private (Token Token, long CurrentLineCount, long CurrentColumnCount) ConsumeNextToken(StreamReader reader, long currentLineCount, long currentColumnCount)
        {
            while (char.IsWhiteSpace((char)reader.Peek()))
            {
                if ((char)reader.Read() == '\n')
                {
                    currentColumnCount = 1;
                    currentLineCount++;
                }
                else
                {
                    currentColumnCount++;
                }

            }

            char? previousChar = null;
            char currentChar = (char)reader.Read();
            char nextChar = (char)reader.Peek();
            currentColumnCount++;

            void AdvanceACharacter()
            {
                previousChar = currentChar;
                currentChar = (char)reader.Read();
                nextChar = (char)reader.Peek();

                currentColumnCount++;
            }            

            if (currentChar == LexerConstants.END_OF_STATEMENT)
            {
                return (new Token(TokenType.Number, LexerConstants.END_OF_STATEMENT, currentLineCount, currentColumnCount - 1), currentLineCount, currentColumnCount);
            }

            if (char.IsDigit(currentChar))
            {
                string res = string.Empty;
              
                if (IsHexIndicator(currentChar, nextChar))
                {
                    while (char.IsLetterOrDigit(currentChar))
                    {
                        res += currentChar;                     
                        AdvanceACharacter();
                    }
                    
                    return (new Token(TokenType.Hexadecimal, res, currentLineCount, currentColumnCount - res.Length), currentLineCount, currentColumnCount);
                }

                while (char.IsDigit(currentChar) || IsDecimalSeparator(currentChar) || IsNumberIndicator(currentChar))
                {
                    res += currentChar;
                    AdvanceACharacter();
                }

                return (new Token(TokenType.Number, res, currentLineCount, currentColumnCount - res.Length), currentLineCount, currentColumnCount);
            }

            if (char.IsLetter(currentChar))
            {
                string res = string.Empty;
                while (char.IsLetterOrDigit(currentChar))
                {
                    res += currentChar;
                    AdvanceACharacter();
                }
                return (new Token(TokenType.Letters, res, currentLineCount, currentColumnCount - res.Length), currentLineCount, currentColumnCount);
            }

            if (reader.EndOfStream)
            {
                return (new Token(TokenType.EndOfFile, currentLineCount, currentColumnCount), currentLineCount, currentColumnCount);
            }

            throw new InvalidOperationException($"Could not determine next token. Should've hit end of file for char: {currentChar} at line {currentLineCount} column {currentColumnCount}?");
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
