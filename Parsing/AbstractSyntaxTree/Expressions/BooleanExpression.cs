using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class BooleanExpression : ExpressionBase
    {
        public BooleanExpression(Token token) : base(token, ExpressionType.BooleanValue)
        {
            Debug.Assert(token.Value is not null);
            Value = (bool)token.Value;
        }

        public bool Value { get; }
    }
}
