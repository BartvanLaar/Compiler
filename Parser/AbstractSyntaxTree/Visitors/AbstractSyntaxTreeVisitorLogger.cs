using Parser.AbstractSyntaxTree.Expressions;

namespace Parser.AbstractSyntaxTree.Visitors
{
    internal class AbstractSyntaxTreeVisitorLogger : IByteCodeGeneratorListener
    {
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
            LogValue(expression);
        }

        public void VisitPrototypeExpression(PrototypeExpression expression)
        {
            Log(expression);
        }

        public void VisitMethodCallExpression(MethodCallExpression expression)
        {
            Log(expression);
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            Log(expression);
        }

        public void VisitAssignmentExpression(AssignmentExpression expression)
        {
            Log(expression);
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            Log(expression);
        }

        public void VisitBodyExpression(BodyExpression expression)
        {
            LogValue(expression.Expression);
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

        private static void Log(ExpressionBase baseExp)
        {
            Console.WriteLine($"Visited tree node of type: '{baseExp?.GetType()?.Name ?? null}'");
        }

    }
}
