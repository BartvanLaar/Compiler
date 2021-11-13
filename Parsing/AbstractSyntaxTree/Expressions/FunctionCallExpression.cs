using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class FunctionCallExpression : ExpressionBase
    {
        public FunctionCallExpression(Token identifierToken, ExpressionBase[] methodArguments) : base(identifierToken, ExpressionType.FunctionCall)
        {

            Arguments = methodArguments;
        }

        public string FunctionName => Token.Name;
        public ExpressionBase[] Arguments { get; }
    }
}
