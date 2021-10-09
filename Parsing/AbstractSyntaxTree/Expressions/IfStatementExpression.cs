namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class IfStatementExpression : ExpressionBase
    {
        public IfStatementExpression(ExpressionBase ifCondition, BodyExpression ifBody, ExpressionBase? @else) : base(ExpressionType.IfStatementExpression)
        {
            IfCondition = ifCondition;
            IfBody = ifBody;
            Else = @else;
        }

        public ExpressionBase IfCondition { get; }
        public ExpressionBase IfBody { get; }
        public ExpressionBase? Else { get; }
    }

    public class BodyExpression : ExpressionBase
    {
        public BodyExpression(IEnumerable<ExpressionBase> expressions) : base(ExpressionType.Body)
        {
            Body = new Queue<ExpressionBase>(expressions);
        }

        public Queue<ExpressionBase> Body { get; }
    }
}
