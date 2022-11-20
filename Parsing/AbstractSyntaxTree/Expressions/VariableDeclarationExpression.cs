using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class VariableDeclarationExpression : ValueExpressionBase
    {

        public VariableDeclarationExpression(Token declarationTypeToken, Token identifierToken, Token assignmentToken, ValueExpressionBase valueExpression) : base(declarationTypeToken, valueExpression.Token)
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
        public ValueExpressionBase ValueExpression { get; }

    }
}
