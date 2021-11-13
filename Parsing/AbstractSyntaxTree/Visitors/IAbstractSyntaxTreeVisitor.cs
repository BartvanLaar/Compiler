using Parsing.AbstractSyntaxTree.Expressions;

namespace Parsing.AbstractSyntaxTree.Visitors
{
    public interface IAbstractSyntaxTreeVisitor
    {
        public string Name { get; }
        void Visit(ExpressionBase expression);
        void VisitValueExpression(ValueExpression expression);
        void VisitBinaryExpression(BinaryExpression expression);
        void VisitFunctionCallExpression(FunctionCallExpression expression);
        void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression);
        void VisitIdentifierExpression(IdentifierExpression expression);
        void VisitReturnExpression(ReturnExpression expression);
        void VisitVariableDeclarationExpression(VariableDeclarationExpression expression);
        void VisitIfStatementExpression(IfStatementExpression expression);
        void VisitForStatementExpression(ForStatementExpression expression);
        void VisitWhileStatementExpression(WhileStatementExpression expression);
        void VisitBodyExpression(BodyExpression expression);
        void VisitDoWhileStatementExpression(DoWhileStatementExpression expression);
    }
}
