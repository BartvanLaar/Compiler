namespace Lexing
{
    public class Token
    {

        public TokenType TokenType { get; set; }
        public int Line { get; init; }
        public int Column { get; init; }
        public string Name { get; init; }
        public string? StringValue { get; set; }
        public float? FloatValue { get; set; } // whats better, float or double?
        public long? IntegerValue { get; set; } // todo: how to handle longs?
        public bool? BooleanValue { get; set; }
        public Token(TokenType tokenType, int line, int column) : this(tokenType, tokenType.ToString(), line, column) { }
        public Token(int line, int column) : this(default, ((TokenType)default).ToString(), line, column) { }
        public Token(TokenType tokenType, char name, int line, int column) : this(tokenType, name.ToString(), line, column) { }
        public Token(TokenType tokenType, string name, int line, int column)
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

        public string ToStringToken()
        {
            return $"{TokenType}";
        }

        public string ToStringValue()
        {
            return $"{StringValue ?? FloatValue ?? IntegerValue as object ?? Name ?? TokenType.ToString()}";

        }

        public override string ToString()
        {
            return $"{ToStringValue()} - {ToStringToken()}";
        }
    }
}
