using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public abstract class ValueExpressionBase : ExpressionBase
    {
        protected ValueExpressionBase(Token token, ExpressionType expressionType) : base(token, expressionType)
        {
        }

        public bool IsNegative { get; set; }

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
