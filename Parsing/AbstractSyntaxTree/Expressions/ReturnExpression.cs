using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class ReturnExpression : ExpressionBase
    {
        public ReturnExpression(Token returnToken, ExpressionBase? expression) : base(returnToken, ExpressionType.Return)
        {
            Expression = expression;
        }

        public ExpressionBase? Expression { get; }
    }
}
