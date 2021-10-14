using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class FunctionCallExpression : ExpressionBase
    {
        public FunctionCallExpression(Token functionIdentifierToken, ExpressionBase[] methodArguments) : base(functionIdentifierToken, ExpressionType.FunctionCall)
        {
            FunctionName = functionIdentifierToken.Name;
            Arguments = methodArguments;
        }

        public string FunctionName { get; }
        public ExpressionBase[] Arguments { get; }
    }
}
