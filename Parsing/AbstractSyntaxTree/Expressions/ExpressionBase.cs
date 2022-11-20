using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public abstract class ValueExpressionBase : ExpressionBase
    {
        protected ValueExpressionBase(Token token, Token typeToken) : base(token)
        {
            TypeToken = typeToken;
        }

        public bool IsNegative { get; set; }
        public Token TypeToken { get; set; }
        public Token ValueToken { get => Token; }
        
    }

    public abstract class ExpressionBase
    {
        protected ExpressionBase(Token token)
        {
            Token = token;
        }

        public Token Token { get; }

    }
}
