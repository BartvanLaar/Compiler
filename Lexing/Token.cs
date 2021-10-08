namespace Lexing
{
    public struct Token
    {

        public TokenType TokenType { get; set; }
        public long Line { get; init; }
        public long Column { get; init; }
        public string Name { get; init; }
        public string? StringValue { get; set; }
        public float? FloatValue { get; set; } // whats better, float or double?
        public long? IntegerValue { get; set; } // todo: how to handle longs?
        public bool? BooleanValue { get; set; }
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

        public string? ToStringToken()
        {
            return $"{TokenType}";
        }

        public string? ToStringValue()
        {
            return $"{StringValue ?? FloatValue ?? IntegerValue as object ?? Name ?? TokenType.ToString()}";

        }

        public override string? ToString()
        {
            return $"{ToStringValue()} - {ToStringToken()}";
        }
    }
}
