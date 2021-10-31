using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class StringExpression : ExpressionBase
    {
        public StringExpression(Token token) : base(token, ExpressionType.String)
        {
            // todo: how to handle nullables?
            Debug.Assert(token.ValueAsString is not null);
            Value = token.ValueAsString;
        }

        public string Value { get; }
    }
}
