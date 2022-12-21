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

            switch (expression)
            {
                case ImportStatementExpression importStatementExpression:
                    visitor.VisitImportExpression(importStatementExpression);
                    return;
                case ContextDefinitionExpression contextDefinitionExpression:
                    visitor.VisitContextExpression(contextDefinitionExpression);
                    return;
                case ClassDefinitionExpression classDefinitionExpression:
                    visitor.VisitClassExpression(classDefinitionExpression);
                    return;
                case EnumDefinitionExpression enumDefinitionExpression:
                    visitor.VisitEnumExpression(enumDefinitionExpression);
                    return;
                case VariableDeclarationExpression variableDeclarationExpression:
                    visitor.VisitVariableDeclarationExpression(variableDeclarationExpression);
                    return;
                case MemberAccessExpression memberAccessExpression:
                    visitor.VisitMemberAccessExpression(memberAccessExpression);
                    return;
                case FunctionDefinitionExpression functionDefinitionExpression:
                    visitor.VisitFunctionDefinitionExpression(functionDefinitionExpression);
                    return;
                case BodyExpression bodyExpression:
                    visitor.VisitBodyExpression(bodyExpression);
                    return;
                case BinaryExpression binaryExpression:
                    visitor.VisitBinaryExpression(binaryExpression);
                    return;
                case ReturnExpression returnExpression:
                    visitor.VisitReturnExpression(returnExpression);
                    return;
                case FunctionCallExpression functionCallExpression:
                    visitor.VisitFunctionCallExpression(functionCallExpression);
                    return;
                case IdentifierExpression identifierExpression:
                    visitor.VisitIdentifierExpression(identifierExpression);
                    return;
                case ObjectInstantiationExpression objectInstantiationExpression:
                    visitor.VisitObjectInstantiationExpression(objectInstantiationExpression);
                    return;
                case ValueExpression valueExpression:
                    visitor.VisitValueExpression(valueExpression);
                    return;
                case IfStatementExpression ifStatementExpression:
                    visitor.VisitIfStatementExpression(ifStatementExpression);
                    return;
                case SwitchStatementExpression switchStatementExpression:
                    visitor.VisitSwitchStatementExpression(switchStatementExpression);
                    return;
                case DoWhileStatementExpression doWhileStatementExpression:
                    visitor.VisitDoWhileStatementExpression(doWhileStatementExpression);
                    return;
                case WhileStatementExpression whileStatementExpression:
                    visitor.VisitWhileStatementExpression(whileStatementExpression);
                    return;
                case ForStatementExpression forStatementExpression:
                    visitor.VisitForStatementExpression(forStatementExpression);
                    return;
                case ForeachStatementExpression foreachStatementExpression:
                    visitor.VisitForeachStatementExpression(foreachStatementExpression);
                    return;
                default:
                    throw new ArgumentException($"Unknown expression type encountered: '{expression.GetType().Name}'");
            }
        }
    }
}
