using Parser.CodeLexer;
using System.Diagnostics;

namespace Parser.AbstractSyntaxTree.Expressions
{
    public sealed class DoubleExpression : ExpressionBase
    {
        public double Value { get; }
        public DoubleExpression(Token token) : base(token, ExpressionType.Double)
        {
            // todo: how to handle nullables?
            Debug.Assert(token.FloatValue.HasValue);
            Value = token.FloatValue.Value;
        }

    }
}
