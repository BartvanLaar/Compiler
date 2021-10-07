namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class IfStatementExpression : ExpressionBase
    {
        public IfStatementExpression(ExpressionBase ifCondition, ExpressionBase then, ExpressionBase @else) : base(ExpressionType.IfStatementExpression)
        {
            IfCondition = ifCondition;
            Then = then;
            Else = @else;
        }

        public ExpressionBase IfCondition { get; }
        public ExpressionBase Then { get; }
        public ExpressionBase Else { get; }
    }
}
