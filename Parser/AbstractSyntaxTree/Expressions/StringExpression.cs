using Parser.CodeLexer;
using System.Diagnostics;

namespace Parser.AbstractSyntaxTree.Expressions
{
    public sealed class StringExpression : ExpressionBase
    {
        public StringExpression(Token token) : base(token, ExpressionType.String)
        {
            // todo: how to handle nullables?
            Debug.Assert(token.StringValue != null);
            Value = token.StringValue;
        }

        public string Value { get; }
    }
}
