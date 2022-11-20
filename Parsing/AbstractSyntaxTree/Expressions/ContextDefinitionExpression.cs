using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class ContextDefinitionExpression : ExpressionBase
    {
        public ContextDefinitionExpression(Token contextIdentifierToken, ContextDefinitionExpression[]? contextExpressions, ClassDefinitionExpression[]? classExpressions, EnumDefinitionExpression[]? enums) : base(contextIdentifierToken)
        {
            Classes = classExpressions ?? Array.Empty<ClassDefinitionExpression>();
            Contexts = contextExpressions ?? Array.Empty<ContextDefinitionExpression>();
            Enums = enums ?? Array.Empty<EnumDefinitionExpression>();
        }

        public Token ContextIdentifierToken => Token;
        public ClassDefinitionExpression[] Classes { get; }
        public EnumDefinitionExpression[] Enums { get; }
        public ContextDefinitionExpression[] Contexts { get; }
    }
}
