using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class FunctionCallExpression : ValueExpressionBase
    {                                                                                                                   // ugly why do i pass a identifierToken?
        public FunctionCallExpression(Token identifierToken, ExpressionBase[] methodArguments) : base(identifierToken, identifierToken , ExpressionType.FunctionCall)
        {

            Arguments = methodArguments;
        }

        public string FunctionName { get => Token.Name; set => Token.Name = value; }
        public ExpressionBase[] Arguments { get; }
    }
}
