using System;

namespace Exceptions
{
    public class SyntaxErrorException : Exception
    {
        public SyntaxErrorException(int line, int column, string message, string filename) : this(new ErrorLocation(line, column, filename), message) { }
        public SyntaxErrorException(ErrorLocation location, string message)
        {
            Location = location;
            Message = $"{Location} -> error: {message}";
        }

        public override string Message { get; }
        public ErrorLocation Location { get; }
    }

    public class ErrorLocation
    {
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
