using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexing
{
    public class Lexer2 : ILexer
    {
        private string filename;
        private string _text;
        private int _lineColumnCounter = 1;
        private int _lineCounter = 1;
        private int _cursor;

        internal Lexer2(string text)
        {
            _text = text;// for now do one entire file at a time...
        }

        public Token ConsumeToken()
        {
            return ConsumeTokens(1)[0];
        }

        public Token[] ConsumeTokens(int amount)
        {
            return TraverseTokens(amount, true);
        }

        public Token PeekToken()
        {
            return PeekTokens(1)[0];
        }

        public Token[] PeekTokens(int amount)
        {
            return TraverseTokens(amount, false);
        }

        public Token[] TraverseTokens(int amount, bool consume)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Can't consume/peek 0 or less tokens at a time.");
            }

            (Token[] tokens, var cursor, var lineCounter, var lineColumnCounter) = TraverseTokensInternal(amount, _cursor, _lineCounter, _lineColumnCounter);

            if (consume)
            {
                _cursor = cursor;
                _lineCounter = lineCounter;
                _lineColumnCounter = lineColumnCounter;
            }

            return tokens;
        }

        internal (Token[] tokens, int Cursor, int Line, int LineColumn) TraverseTokensInternal(int amount, int cursor, int lineCounter, int lineColumnCounter)
        {
            var tokens = new Token[amount];
            for (int counter = 0; counter < amount; counter++)
            {
                (Token token, cursor, lineCounter, lineColumnCounter) =
                cursor < _text.Length
                ? GetNextToken(cursor, lineCounter, lineColumnCounter)
                : (new Token(TokenType.EndOfFile, lineCounter, lineColumnCounter), cursor, lineCounter, lineColumnCounter);

                tokens[counter] = token;
            }

            return (tokens, cursor, lineCounter, lineColumnCounter);
        }

        private (Token Token, int Cursor, int LineCounter, int Column)  GetNextToken(int cursor, int lineCount, int lineColumnCount)
        {
            // Make it so it returns bug when not all cases are hit.
            Token token = new Token(TokenType.Bug,lineCount, lineColumnCount);
            // white spaces are either part of a token, or to be skipped over..
            (cursor, lineCount, lineColumnCount) = SkipWhiteSpaces(cursor, lineCount, lineColumnCount);
            // get next comment
            // get next block of comments instead?
            // get next 'summary' i.e. /* ... */

            return (token, cursor, lineCount, lineColumnCount);
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

    }
}
