using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class BooleanExpression : ExpressionBase
    {
        public BooleanExpression(Token token) : base(token, ExpressionType.BooleanValue)
        {
            Debug.Assert(token.BooleanValue.HasValue);
            Value = token.BooleanValue.Value;
        }

        public bool Value { get; }
    }
}
