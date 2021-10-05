using Parser.CodeLexer;

namespace Parser.AbstractSyntaxTree.Expressions
{
    internal abstract class ExpressionBase
    {
        protected ExpressionBase(ExpressionType expressionType) : this(null, expressionType) { }

        protected ExpressionBase(Token? token, ExpressionType expressionType)
        {
            Token = token;
            NodeExpressionType = expressionType;
        }

        public Token? Token { get; }
        public ExpressionType NodeExpressionType { get; }

    }
}
