using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    // e.g. a body of an if statement, while loop, for loop, function/method, etc.
    public sealed class BodyExpression : ExpressionBase
    {
        public BodyExpression(Token parentToken,IEnumerable<ExpressionBase> expressions) : base(parentToken)
        {
            Body = expressions as ExpressionBase[] ?? expressions.ToArray();
        }

        public ExpressionBase[] Body { get; }

    }
}
