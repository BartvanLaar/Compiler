using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class EnumDefinitionExpression : ExpressionBase
    {
        public EnumDefinitionExpression(Token identifierToken, BodyExpression enumValuesBody): base(identifierToken)
        {
            EnumValuesBody = enumValuesBody;
        }

        public BodyExpression EnumValuesBody { get; }
    }
}
