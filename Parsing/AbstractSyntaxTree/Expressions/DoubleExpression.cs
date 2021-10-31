using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class DoubleExpression : ExpressionBase
    {
        public double Value { get; }
        public DoubleExpression(Token token) : base(token, ExpressionType.Double)
        {
            // todo: how to handle nullables?
            Debug.Assert(token.Value is not null);
            Value = (double)token.Value;
        }

    }
}
