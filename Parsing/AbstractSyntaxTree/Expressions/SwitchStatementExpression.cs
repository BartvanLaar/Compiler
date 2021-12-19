using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class SwitchStatementExpression : ExpressionBase
    {
        public SwitchStatementExpression(Token token, ValueExpressionBase switchValue, SwitchCase[] cases) : base(token)
        {
            SwitchValue = switchValue;
            Cases = cases;
        }
        public override ExpressionType DISCRIMINATOR => ExpressionType.Switch;

        public ValueExpressionBase SwitchValue { get; }
        public SwitchCase[] Cases { get; }
    }

    public class SwitchCase
    {
        public ValueExpressionBase ValueToCheckAgainst { get; set; }
        public BodyExpression Body { get; set; }
    }
}
