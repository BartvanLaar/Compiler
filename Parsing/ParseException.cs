using Lexing;
using System;

namespace Parsing
{
    public class ParseException : Exception
    {
        internal ParseException(Token token, string message, string filename) : this(new ErrorLocation(token, filename), message) { }
        internal ParseException(ErrorLocation location, string message)
        {
            Location = location;
            Message = $"{Location} -> error: {message}";
        }

        public override string Message { get; }
        public ErrorLocation Location { get; set; }
    }

    public class ErrorLocation
    {
        internal ErrorLocation(Token token, string filepath) : this(token.Line, token.Column, filepath) { }
        internal ErrorLocation(int line, int column, string filepath)
        {
            Line = line;
            Column = column;
            File = filepath;
        }

        public int Line { get; }
        public int Column { get; }
        public string File { get; }

        public override string ToString()
        {
            return $"{File}:{Line}:{Column}";
        }
    }

}
