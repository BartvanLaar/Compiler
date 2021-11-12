namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class WhileStatementExpression : ExpressionBase
    {
        public WhileStatementExpression(ExpressionBase whileCondition, BodyExpression doBody) : base(ExpressionType.WhileStatementExpression)
        {
            Condition = whileCondition;
            Body = doBody;
        }

        public ExpressionBase Condition { get; set; }
        public ExpressionBase Body {  get; set; }
    }
}
