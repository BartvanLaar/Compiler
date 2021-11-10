namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class DoWhileStatementExpression : ExpressionBase
    {
        public DoWhileStatementExpression(ExpressionBase whileCondition, BodyExpression doBody) : base(ExpressionType.DoWhileStatementExpression)
        {
            Condition = whileCondition;
            DoBody = doBody;
        }

        public ExpressionBase Condition { get; set; }
        public ExpressionBase DoBody {  get; set; }
    }
}
