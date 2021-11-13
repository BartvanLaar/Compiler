using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
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
