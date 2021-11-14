namespace Lexing
{
    public class Token
    {
        public Token(TokenType tokenType, int line, int column) : this(tokenType, tokenType.ToString(), line, column) { }
        public Token(TokenType tokenType, string name, int line, int column)
        {
            TokenType = tokenType;
            Line = line;
            Column = column;
            Name = name;
        }

        public Token(Token token)
        {
            TokenType = token.TokenType;
            TypeIndicator = token.TypeIndicator;
            Line = token.Line;
            Column = token.Column;
            Name = token.Name;
            Value = token.Value;
        }

        public TokenType TokenType { get; set; }
        public TypeIndicator TypeIndicator { get; set; }
        public int Line { get; init; }
        public int Column { get; init; }
        public string Name { get; set; }
        public object? Value { get; set; }        
        public string? ValueAsString => Value?.ToString();

        private string ToStringToken()
        {
            return $"{TokenType}";
        }

        private string ToStringValue()
        {
            return $"{Value ?? Value ?? Value as object ?? Name ?? TokenType.ToString()}";

        }

        public override string ToString()
        {
            return $"{ToStringValue()} - {ToStringToken()}";
        }
    }
}
