using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class DoWhileStatementExpression : ExpressionBase
    {
        public DoWhileStatementExpression(Token whileToken, ExpressionBase condition, BodyExpression doBody) : base(whileToken)
        {
            Condition = condition;
            Body = doBody;
        }

        public ExpressionBase Condition { get; set; }
        public ExpressionBase Body {  get; set; }
    }
}
