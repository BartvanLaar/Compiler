using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class NegativeValueExpression : ExpressionBase
    {
        public NegativeValueExpression(Token token, ExpressionBase valueExpression) : base(token, ExpressionType.NegativeValue)
        {
            ValueExpression = valueExpression;
        }

        public ExpressionBase ValueExpression { get; }
    }
}
