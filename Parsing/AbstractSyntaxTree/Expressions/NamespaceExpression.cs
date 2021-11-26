using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    internal class NamespaceExpression : ExpressionBase
    {
        public NamespaceExpression(Token token) : base(token)
        {
        }

        public override ExpressionType DISCRIMINATOR => ExpressionType.Namespace;
    }
}
