using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class MethodCallExpression : ExpressionBase
    {
        public MethodCallExpression(Token token, ExpressionBase[] methodArguments) : base(token, ExpressionType.MethodCall)
        {
            Callee = token.Name;
            MethodArguments = methodArguments;
        }

        public string Callee { get; }
        public ExpressionBase[] MethodArguments { get; }
    }
}
