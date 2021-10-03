namespace Parser.CodeLexer
{
    internal struct Token
    {

        public TokenType TokenType { get; internal set; }
        public long Line { get; init; }
        public long Column { get; init; }
        public string Name { get; init; }
        public string? StringValue { get; internal set; }
        public float? FloatValue { get; internal set; }
        public int? IntegerValue { get; internal set; } // todo: how to handle longs?
        public bool? BooleanValue { get; internal set; }
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

        public override string? ToString()
        {
            return $"{StringValue ?? FloatValue ?? IntegerValue as object ?? Name ?? TokenType.ToString()} - {TokenType}";
        }
    }
}
