namespace Parsing.AbstractSyntaxTree.Expressions
{
    // e.g. a body of an if statement, while loop, for loop, function/method, etc.
    public class BodyExpression : ExpressionBase
    {
        public BodyExpression(IEnumerable<ExpressionBase> expressions) : base(ExpressionType.Body)
        {
            Body = new Queue<ExpressionBase>(expressions);
        }

        public Queue<ExpressionBase> Body { get; }
    }
}
