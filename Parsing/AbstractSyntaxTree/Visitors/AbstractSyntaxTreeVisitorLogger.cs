using Parsing.AbstractSyntaxTree.Expressions;

namespace Parsing.AbstractSyntaxTree.Visitors
{
    public class AbstractSyntaxTreeVisitorLogger : IAbstractSyntaxTreeVisitor
    {
        public string Name => "AST Logger";

        public void Visit(ExpressionBase? expression) => AbstractSyntaxTreeVisitor.Visit(this, expression);
        private void Visit(ExpressionBase?[] expressions)
        {
            foreach (var expr in expressions)
            {
                Visit(expr);
            }
        }
        public void VisitValueExpression(ValueExpression expression)
        {
            LogValue(expression);
        }
       
        public void VisitBinaryExpression(BinaryExpression expression)
        {
            Visit(expression.LeftHandSide);
            Visit(expression.RightHandSide);
            Console.WriteLine($"Visiting {expression.GetType()} of TokenType {expression.Token?.ToStringToken()} resulting in {expression.LeftHandSide.Token?.ToStringValue()} {expression.Token?.ToStringValue()} {expression.RightHandSide?.Token?.ToStringValue()}.");
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            Visit(expression.Arguments);
            Log(expression);
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            Visit(expression.Body);
            Log(expression);
        }

        public void VisitVariableDeclarationExpression(VariableDeclarationExpression expression)
        {
            Visit(expression.ValueExpression);
            Log(expression);
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            Log(expression);
        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            Visit(expression.IfCondition);
            Visit(expression.IfBody);
            Visit(expression.ElseBody);
            Log(expression);
        }

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            Log(expression);
            throw new NotImplementedException();
        }

        public void VisitBodyExpression(BodyExpression expression)
        {
            Visit(expression.Body);
            Log(expression);
        }

        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            Log(expression);
        }

        public void VisitDoWhileStatementExpression(DoWhileStatementExpression expression)
        {
            Log(expression);
        }

        public void VisitReturnExpression(ReturnExpression expression)
        {
            Visit(expression.ReturnExpr);
            Log(expression);
        }

        private static void LogValue(ExpressionBase baseExp)
        {
            Console.WriteLine($"Visited tree node of type: '{baseExp?.GetType()?.Name ?? null}' with token: '{baseExp?.Token}'.");
        }

        private static void Log(ExpressionBase baseExp)
        {
            Console.WriteLine($"Visited tree node of type: '{baseExp?.GetType()?.Name ?? null}'");
        }


    }
}
