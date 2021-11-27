using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class NamespaceDefinitionExpression : ExpressionBase
    {
        public NamespaceDefinitionExpression(Token namespaceNameToken, ClassDefinitionExpression[] classExpressions) : base(namespaceNameToken)
        {
            Classes = classExpressions;
        }

        public Token NamespaceNameToken => Token;
        public ClassDefinitionExpression[] Classes { get; }

        public override ExpressionType DISCRIMINATOR => ExpressionType.Namespace;
    }
}
