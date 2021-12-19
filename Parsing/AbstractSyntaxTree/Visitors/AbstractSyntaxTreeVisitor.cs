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
                case ExpressionType.Import:
                    visitor.VisitImportExpression((ImportStatementExpression)expression);
                    return;
                case ExpressionType.Namespace:
                    visitor.VisitNamespaceExpression((NamespaceDefinitionExpression)expression);
                    return;
                case ExpressionType.Class:
                    visitor.VisitClassExpression((ClassDefinitionExpression)expression);
                    return;
                case ExpressionType.Enum:
                    visitor.VisitEnumExpression((EnumDefinitionExpression)expression);
                    return;
                case ExpressionType.VariableDeclaration:
                    visitor.VisitVariableDeclarationExpression((VariableDeclarationExpression)expression);
                    return;
                case ExpressionType.MemberAccess:
                    visitor.VisitMemberAccessExpression((MemberAccessExpression)expression);
                    return;
                case ExpressionType.FunctionDefinition:
                    visitor.VisitFunctionDefinitionExpression((FunctionDefinitionExpression)expression);
                    return;
                case ExpressionType.Body:
                    visitor.VisitBodyExpression((BodyExpression)expression);
                    return;
                case ExpressionType.Binary:
                    visitor.VisitBinaryExpression((BinaryExpression)expression);
                    return;
                case ExpressionType.Return:
                    visitor.VisitReturnExpression((ReturnExpression)expression);
                    return;
                case ExpressionType.FunctionCall:
                    visitor.VisitFunctionCallExpression((FunctionCallExpression)expression);
                    return;
                case ExpressionType.Identifier:
                    visitor.VisitIdentifierExpression((IdentifierExpression)expression);
                    return;
                case ExpressionType.ObjectInstantiation:
                    visitor.VisitObjectInstantiationExpression((ObjectInstantiationExpression)expression);
                    return;
                case ExpressionType.Value:
                    visitor.VisitValueExpression((ValueExpression)expression);
                    return;
                case ExpressionType.If:
                    visitor.VisitIfStatementExpression((IfStatementExpression)expression);
                    return;
                case ExpressionType.Switch:
                    visitor.VisitSwitchStatementExpression((SwitchStatementExpression)expression);
                    return;
                case ExpressionType.DoWhile:
                    visitor.VisitDoWhileStatementExpression((DoWhileStatementExpression)expression);
                    return;
                case ExpressionType.While:
                    visitor.VisitWhileStatementExpression((WhileStatementExpression)expression);
                    return;
                case ExpressionType.For:
                    visitor.VisitForStatementExpression((ForStatementExpression)expression);
                    return;
                case ExpressionType.Foreach:
                    visitor.VisitForeachStatementExpression((ForeachStatementExpression)expression);
                    return;
                default:
                    throw new ArgumentException($"Unknown expression type encountered: '{expression.DISCRIMINATOR}'");
            }
        }
    }
}
