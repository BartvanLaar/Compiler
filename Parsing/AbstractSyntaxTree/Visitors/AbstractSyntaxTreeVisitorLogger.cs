using Parsing.AbstractSyntaxTree.Expressions;

namespace Parsing.AbstractSyntaxTree.Visitors
{
    public class AbstractSyntaxTreeVisitorLogger : IByteCodeGeneratorListener
    {
        public void VisitBooleanExpression(BooleanExpression expression)
        {
            LogValue(expression);
        }

        public void VisitDoubleExpression(DoubleExpression expression)
        {
            LogValue(expression);
        }

        public void VisitFloatExpression(FloatExpression expression)
        {
            LogValue(expression);
        }

        public void VisitIntegerExpression(IntegerExpression expression)
        {
            LogValue(expression);
        }

        public void VisitStringExpression(StringExpression expression)
        {
            LogValue(expression);
        }
        public void VisitCharacterExpression(CharacterExpression expression)
        {
            LogValue(expression);
        }

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            Console.WriteLine($"Visiting {expression.GetType()} of TokenType {expression.Token?.ToStringToken()} resulting in {expression.LeftHandSide.Token?.ToStringValue()} {expression.Token?.ToStringValue()} {expression.RightHandSide?.Token?.ToStringValue()}.");
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            Log(expression);
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            Log(expression);
        }

        public void VisitVariableDeclarationExpression(VariableDeclarationExpression expression)
        {
            Log(expression);
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            Log(expression);
        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            Log(expression);
        }

        private static void LogValue(ExpressionBase baseExp)
        {
            Console.WriteLine($"Visited tree node of type: '{baseExp?.GetType()?.Name ?? null}' with token: '{baseExp?.Token}'.");
        }

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            Log(expression);
        }

        public void VisitBodyStatementExpression(BodyExpression expression)
        {
            Log(expression);
        }

        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            Log(expression);
        }

        private static void Log(ExpressionBase baseExp)
        {
            Console.WriteLine($"Visited tree node of type: '{baseExp?.GetType()?.Name ?? null}'");
        }

        public void VisitReturnExpression(ReturnExpression expression)
        {
            Log(expression);
        }
    }
}
