using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    internal class ClassExpression : ExpressionBase
    {
        public ClassExpression(VariableDeclarationExpression[] variableDeclarationExpressions, FunctionDefinitionExpression[] functionDefinitionExpressions, ClassExpression[] classExpressions, Token token) : base(token)
        {           
            Variables = variableDeclarationExpressions;
            Functions = functionDefinitionExpressions;
            Classes = classExpressions;
        }

        public VariableDeclarationExpression[] Variables { get; }
        public FunctionDefinitionExpression[] Functions { get; }
        public ClassExpression[] Classes { get; }

        public override ExpressionType DISCRIMINATOR => ExpressionType.Class;
    }
}
