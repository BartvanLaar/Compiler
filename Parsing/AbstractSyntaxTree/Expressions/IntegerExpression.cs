using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class IntegerExpression : ExpressionBase
    {
        public IntegerExpression(Token token) : base(token, ExpressionType.Integer)
        {
            // todo: how to handle nullables?
            Debug.Assert(token.IntegerValue.HasValue);
            Value = token.IntegerValue.Value;
        }
        public int Value { get; }

    }
}
