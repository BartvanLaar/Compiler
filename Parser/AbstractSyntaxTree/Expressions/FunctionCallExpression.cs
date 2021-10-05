namespace Parser.AbstractSyntaxTree.Expressions
{
    internal sealed class FunctionCallExpression : ExpressionBase
    {
        public FunctionCallExpression(PrototypeExpression prototype, ExpressionBase body) : base(ExpressionType.FunctionCall)
        {
            Prototype = prototype;
            Body = new BodyExpression(body);
        }

        public PrototypeExpression Prototype { get; }
        public BodyExpression Body { get; }
    }

    internal sealed class BodyExpression : ExpressionBase
    {
        public BodyExpression(ExpressionBase expression) : base(ExpressionType.Body)
        {
            Expression = expression;
        }

        public ExpressionBase Expression { get; }
    }
}
