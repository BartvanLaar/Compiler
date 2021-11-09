namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class WhileStatementExpression : ExpressionBase
    {
        public WhileStatementExpression(ExpressionBase whileCondition, BodyExpression doBody, bool runDoFirst = false) : base(ExpressionType.WhileStatementExpression)
        {
            WhileCondition = whileCondition;
            DoBody = doBody;
            RunDoFirst = runDoFirst;
        }

        public ExpressionBase WhileCondition { get; set; }
        public ExpressionBase DoBody {  get; set; }
        public bool RunDoFirst { get; }
    }
}
