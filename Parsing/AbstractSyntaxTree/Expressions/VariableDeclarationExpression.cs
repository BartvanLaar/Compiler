using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class VariableDeclarationExpression : ExpressionBase
    {

        public VariableDeclarationExpression(Token declarationTypeToken, Token identifierToken, Token assignmentToken, ExpressionBase valueExpression) : base(declarationTypeToken, ExpressionType.VariableDeclaration) // todo: do we/should we pass a token to the base?
        {
            DeclarationTypeToken = declarationTypeToken;
            IdentifierToken = identifierToken;
            AssignmentToken = assignmentToken;
            ValueExpression = valueExpression;
        }

        public Token DeclarationTypeToken { get; }
        public Token AssignmentToken { get; }
        public Token IdentifierToken { get; }
        public string Identifier => IdentifierToken.Name;
        public ExpressionBase ValueExpression { get; }

    }
}
