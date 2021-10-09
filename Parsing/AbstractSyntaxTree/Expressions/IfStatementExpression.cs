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
}
