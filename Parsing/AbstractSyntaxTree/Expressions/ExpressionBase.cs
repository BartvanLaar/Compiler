using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public abstract class ValueExpressionBase : ExpressionBase
    {
        protected ValueExpressionBase(Token token, Token typeToken, ExpressionType expressionType) : base(token, expressionType)
        {
            TypeToken = typeToken;
        }

        public bool IsNegative { get; set; }
        public Token TypeToken { get; set; }
        public Token ValueToken { get => Token; }
    }

    public abstract class ExpressionBase
    {
        protected ExpressionBase(Token token, ExpressionType expressionType)
        {
            Token = token;
            NodeExpressionType = expressionType;
        }


        public Token Token { get; }
        public ExpressionType NodeExpressionType { get; }

    }
}
