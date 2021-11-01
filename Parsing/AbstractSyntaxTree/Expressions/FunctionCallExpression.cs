using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class FunctionCallExpression : ExpressionBase
    {
        public FunctionCallExpression(string Name, ExpressionBase[] methodArguments) : base(ExpressionType.FunctionCall)
        {
            FunctionName = Name;
            Arguments = methodArguments;
        }

        public string FunctionName { get; }
        public ExpressionBase[] Arguments { get; }
    }
}
