using Parsing.AbstractSyntaxTree.Expressions;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Visitors
{
    public static class AbstractSyntaxTreeVisitor
    {
        public static void Visit(ExpressionBase[] abstractSyntaxTrees, IAbstractSyntaxTreeVisitor visitor)
        {
            foreach (var expr in abstractSyntaxTrees)
            {
                Debug.Assert(expr is not null, "Top level expressions should never be null!");
                Visit(visitor, expr);
            }
        }

        public static void Visit(IAbstractSyntaxTreeVisitor visitor, ExpressionBase? expression)
        {
            if (expression is null)
            {
                return;
            }

            switch (expression.DISCRIMINATOR)
            {
                case ExpressionType.Binary:
                    visitor.VisitBinaryExpression((BinaryExpression)expression);
                    break;
                case ExpressionType.VariableDeclaration:
                    visitor.VisitVariableDeclarationExpression((VariableDeclarationExpression)expression);
                    break;
                case ExpressionType.Return:
                    visitor.VisitReturnExpression((ReturnExpression)expression);
                    break;
                case ExpressionType.FunctionCall:
                    visitor.VisitFunctionCallExpression((FunctionCallExpression)expression);
                    break;
                case ExpressionType.Identifier:
                    visitor.VisitIdentifierExpression((IdentifierExpression)expression);
                    break;
                case ExpressionType.FunctionDefinition:
                    visitor.VisitFunctionDefinitionExpression((FunctionDefinitionExpression)expression);
                    break;
                case ExpressionType.Value:
                    visitor.VisitValueExpression((ValueExpression)expression);
                    break;
                case ExpressionType.If:
                    visitor.VisitIfStatementExpression((IfStatementExpression)expression);
                    break;
                case ExpressionType.DoWhile:
                    visitor.VisitDoWhileStatementExpression((DoWhileStatementExpression)expression);
                    break;
                case ExpressionType.While:
                    visitor.VisitWhileStatementExpression((WhileStatementExpression)expression);
                    break;
                case ExpressionType.Body://todo: fix
                    visitor.VisitBodyExpression((BodyExpression)expression);
                    break;
                case ExpressionType.For:
                    visitor.VisitForStatementExpression((ForStatementExpression)expression);
                    break;
                default:
                    // should this be visiting a top level?
                    throw new ArgumentException($"Unknown expression type encountered: '{expression.Token.TokenType}'");
            }
        }
    }
}
