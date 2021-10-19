using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class VariableDeclarationExpression : ExpressionBase
    {

        public VariableDeclarationExpression(Token declarationTypeToken, ExpressionBase identificationExpression, Token assignmentToken, ExpressionBase valueExpression) : base(declarationTypeToken, ExpressionType.VariableDeclaration) // todo: do we/should we pass a token to the base?
        {
            DeclarationTypeToken = declarationTypeToken;
            IdentificationExpression = identificationExpression;
            AssignmentToken = assignmentToken;
            ValueExpression = valueExpression;
        }

        public Token DeclarationTypeToken { get; }
        public Token AssignmentToken { get; }
        public ExpressionBase IdentificationExpression { get; }
        public ExpressionBase ValueExpression { get; }

    }
}
