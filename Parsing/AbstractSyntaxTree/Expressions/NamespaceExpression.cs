using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    internal class NamespaceExpression : ExpressionBase
    {
        public NamespaceExpression(Token token, ClassExpression[] classExpressions) : base(token)
        {
            Classes = classExpressions;
        }

        public ClassExpression[] Classes { get; }

        public override ExpressionType DISCRIMINATOR => ExpressionType.Namespace;
    }
}
