namespace Lexing
{
    public class Token
    {
        public TokenType TokenType { get; set; }
        public TypeIndicator TypeIndicator { get; set; }
        public int Line { get; init; }
        public int Column { get; init; }
        public string Name { get; init; }
        public object? Value { get; set; }        
        public string? ValueAsString => Value?.ToString();
        public Token(TokenType tokenType, int line, int column) : this(tokenType, tokenType.ToString(), line, column) { }
        public Token(int line, int column) : this(default, ((TokenType)default).ToString(), line, column) { }
        public Token(TokenType tokenType, char name, int line, int column) : this(tokenType, name.ToString(), line, column) { }
        public Token(TokenType tokenType, string name, int line, int column)
        {
            TokenType = tokenType;
            Line = line;
            Column = column;
            Name = name;
        }

        public string ToStringToken()
        {
            return $"{TokenType}";
        }

        public string ToStringValue()
        {
            return $"{Value ?? Value ?? Value as object ?? Name ?? TokenType.ToString()}";

        }

        public override string ToString()
        {
            return $"{ToStringValue()} - {ToStringToken()}";
        }
    }
}
