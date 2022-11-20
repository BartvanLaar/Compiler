using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class IfStatementExpression : ExpressionBase
    {
        public IfStatementExpression(Token ifToken, ExpressionBase ifCondition, BodyExpression ifBody, ExpressionBase? @else) : base(ifToken)
        {
            IfCondition = ifCondition;
            IfBody = ifBody;
            ElseBody = @else;
        }

        public ExpressionBase IfCondition { get; }
        public ExpressionBase IfBody { get; }
        public ExpressionBase? ElseBody { get; }
    }   
}
