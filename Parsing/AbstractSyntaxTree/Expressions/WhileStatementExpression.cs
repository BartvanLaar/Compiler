namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class WhileStatementExpression : ExpressionBase
    {
        public WhileStatementExpression(ExpressionBase whileCondition, BodyExpression doBody) : base(ExpressionType.WhileStatementExpression)
        {
            WhileCondition = whileCondition;
            DoBody = doBody;
        }

        public ExpressionBase WhileCondition { get; set; }
        public ExpressionBase DoBody {  get; set; }
    }
}
