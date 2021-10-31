using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class CharacterExpression : ExpressionBase
    {
        public CharacterExpression(Token token) : base(token, ExpressionType.Character)
        {
            Debug.Assert(token.ValueAsString is not null);
            Value = token.ValueAsString;
        }

        public string Value { get; }
    }
}
