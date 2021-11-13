using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class ValueExpression : ExpressionBase
    {
        public ValueExpression(Token token) : base(token, ExpressionType.Float)
        {
            // todo: how to handle nullables?
            Debug.Assert(token.Value is not null);
            //Value = (float)token.Value;
        }
        public object Value { get; }
    }
}
