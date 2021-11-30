using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class NamespaceDefinitionExpression : ExpressionBase
    {
        public NamespaceDefinitionExpression(Token namespaceNameToken, NamespaceDefinitionExpression[]? namespaceExpressions, ClassDefinitionExpression[]? classExpressions) : base(namespaceNameToken)
        {
            Classes = classExpressions ?? Array.Empty<ClassDefinitionExpression>();
            Namespaces = namespaceExpressions ?? Array.Empty<NamespaceDefinitionExpression>();
        }

        public Token NamespaceNameToken => Token;
        public ClassDefinitionExpression[] Classes { get; }
        public NamespaceDefinitionExpression[] Namespaces { get; }

        public override ExpressionType DISCRIMINATOR => ExpressionType.Namespace;
    }
}
