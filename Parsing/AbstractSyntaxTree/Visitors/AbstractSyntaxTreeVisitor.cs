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
            if(expression is null)
            {
                return;
            }

            switch (expression.NodeExpressionType)
            {
                // These are all handled by binary operator expressions.
                case ExpressionType.LogicalAnd:
                case ExpressionType.ConditionalAnd:
                case ExpressionType.LogicalOr:
                case ExpressionType.ConditionalOr:
                    visitor.VisitBinaryExpression((BinaryExpression)expression);
                    break;
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.DivideRest:
                    visitor.VisitBinaryExpression((BinaryExpression)expression);
                    break;
                // These are all handled by binary operator expressions.
                case ExpressionType.Assignment:
                case ExpressionType.Equivalent:
                case ExpressionType.Equals:
                case ExpressionType.NotEquivalent:
                case ExpressionType.NotEquals:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanEqual:
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
                case ExpressionType.Double:
                    visitor.VisitDoubleExpression((DoubleExpression)expression);
                    break;
                case ExpressionType.Float:
                    visitor.VisitFloatExpression((FloatExpression)expression);
                    break;
                case ExpressionType.Integer:
                    visitor.VisitIntegerExpression((IntegerExpression)expression);
                    break;
                case ExpressionType.BooleanValue:
                    visitor.VisitBooleanExpression((BooleanExpression)expression);
                    break;
                case ExpressionType.String:
                    visitor.VisitStringExpression((StringExpression)expression);
                    break;
                case ExpressionType.Character:
                    visitor.VisitCharacterExpression((CharacterExpression)expression);
                    break;
                case ExpressionType.IfStatementExpression:
                    visitor.VisitIfStatementExpression((IfStatementExpression)expression);
                    break;
                case ExpressionType.WhileStatementExpression:
                    visitor.VisitWhileStatementExpression((WhileStatementExpression)expression);
                    break;
                case ExpressionType.Body:
                    visitor.VisitBodyExpression((BodyExpression)expression);
                    break;
                case ExpressionType.ForStatementExpression:
                    visitor.VisitForStatementExpression((ForStatementExpression)expression);
                    break;
                default:
                    // should this be visiting a top level?
                    throw new ArgumentException($"Unknown expression type encountered: '{expression.NodeExpressionType}'");
            }
        }
    }
}
