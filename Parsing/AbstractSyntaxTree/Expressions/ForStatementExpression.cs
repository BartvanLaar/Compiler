using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class ForStatementExpression : ExpressionBase
    {
        public ForStatementExpression(Token forToken, VariableDeclarationExpression variableDeclaration, ExpressionBase conditionExpression, ExpressionBase variableIncreaseExpression, BodyExpression forBody) : base(forToken, ExpressionType.ForStatementExpression)
        {
            VariableDeclaration = variableDeclaration;
            VariableName = variableDeclaration.Identifier;
            Condition = conditionExpression;
            VariableIncreaseExpression = variableIncreaseExpression;
            Body = forBody;
        }

        public string VariableName { get; }
        public ExpressionBase Condition { get; }
        public ExpressionBase VariableDeclaration { get; }
        public ExpressionBase VariableIncreaseExpression { get; }
        public ExpressionBase Body { get; }
    }
}
