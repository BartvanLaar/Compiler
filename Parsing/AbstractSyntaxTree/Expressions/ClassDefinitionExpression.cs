using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class ClassDefinitionExpression : ExpressionBase
    {
        public ClassDefinitionExpression(Token classIdentifier, VariableDeclarationExpression[] variableDeclarationExpressions, FunctionDefinitionExpression[] functionDefinitionExpressions, ClassDefinitionExpression[] classExpressions, EnumDefinitionExpression[] enums) : base(classIdentifier)
        {
            Variables = variableDeclarationExpressions;
            Functions = functionDefinitionExpressions;
            Classes = classExpressions;
            Enums = enums;
        }

        public Token ClassName => Token;
        public VariableDeclarationExpression[] Variables { get; }
        public FunctionDefinitionExpression[] Functions { get; }
        public ClassDefinitionExpression[] Classes { get; }
        public EnumDefinitionExpression[] Enums { get; }
    }
}
