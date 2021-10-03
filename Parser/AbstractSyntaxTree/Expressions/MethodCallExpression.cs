using Parser.Lexer;

namespace Parser.AbstractSyntaxTree.Expressions
{
    internal sealed class MethodCallExpression : ExpressionBase
    {
        public MethodCallExpression(Token token, ExpressionBase[] methodArguments) : base(token, ExpressionType.MethodCall)
        {
            Callee = token.Name;
            MethodArguments = methodArguments;
        }

        public string Callee { get; }
        public ExpressionBase[] MethodArguments { get; }

        protected internal override ExpressionBase Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitMethodCallExpressionAST(this);
        }
    }
}
