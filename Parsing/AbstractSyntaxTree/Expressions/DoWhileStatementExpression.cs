namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class DoWhileStatementExpression : ExpressionBase
    {
        public DoWhileStatementExpression(ExpressionBase whileCondition, BodyExpression doBody) : base(ExpressionType.DoWhileStatementExpression)
        {
            Condition = whileCondition;
            Body = doBody;
        }

        public ExpressionBase Condition { get; set; }
        public ExpressionBase Body {  get; set; }
    }
}
