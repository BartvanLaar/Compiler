using Parser.CodeLexer;
using System.Diagnostics;

namespace Parser.AbstractSyntaxTree.Expressions
{
    public sealed class CharacterExpression : ExpressionBase
    {
        public CharacterExpression(Token token) : base(token, ExpressionType.Character)
        {
            Debug.Assert(token.StringValue != null);
            Value = token.StringValue.FirstOrDefault();//todo: handle invalid character lengths...
        }

        public char Value { get; }
    }
}
