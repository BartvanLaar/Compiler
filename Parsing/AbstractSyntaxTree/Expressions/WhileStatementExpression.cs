using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class WhileStatementExpression : ExpressionBase
    {
        public WhileStatementExpression(Token whileToken, ExpressionBase whileCondition, BodyExpression doBody) : base(whileToken)
        {
            Condition = whileCondition;
            Body = doBody;
        }

        public ExpressionBase Condition { get; set; }
        public ExpressionBase Body {  get; set; }

    }
}
