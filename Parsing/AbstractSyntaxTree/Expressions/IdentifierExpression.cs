using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class IdentifierExpression : ValueExpressionBase
    {
        public IdentifierExpression(Token token) : base(token, token)
        {
            Identifier = token.Name;
        }

        public string Identifier { get; }

        public override ExpressionType DISCRIMINATOR => ExpressionType.Identifier;
    }
}
