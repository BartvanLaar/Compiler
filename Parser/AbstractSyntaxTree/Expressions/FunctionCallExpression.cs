namespace Parser.AbstractSyntaxTree.Expressions
{
    internal sealed class FunctionCallExpression : ExpressionBase
    {
        public FunctionCallExpression(PrototypeExpression prototype, ExpressionBase body) : base(ExpressionType.FunctionCall)
        {
            Prototype = prototype;
            Body = body;
        }

        public PrototypeExpression Prototype { get; }
        public ExpressionBase Body { get; }

        protected internal override ExpressionBase Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitFunctionCallExpression(this);
        }
    }
}
