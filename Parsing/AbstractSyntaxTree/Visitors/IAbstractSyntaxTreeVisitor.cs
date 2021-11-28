using Parsing.AbstractSyntaxTree.Expressions;

namespace Parsing.AbstractSyntaxTree.Visitors
{
    public interface IAbstractSyntaxTreeVisitor
    {
        public string Name { get; }

        void Initialize(IReadOnlyDictionary<string, IScope> scopes);
        void Visit(ExpressionBase expression);
        void VisitNamespaceExpression(NamespaceDefinitionExpression expression);
        void VisitClassExpression(ClassDefinitionExpression expression);
        void VisitImportExpression(ImportStatementExpression expression);
        void VisitVariableDeclarationExpression(VariableDeclarationExpression expression);
        void VisitMemberAccessExpression(MemberAccessExpression expression);
        void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression);
        void VisitBodyExpression(BodyExpression expression);
        void VisitValueExpression(ValueExpression expression);
        void VisitBinaryExpression(BinaryExpression expression);
        void VisitFunctionCallExpression(FunctionCallExpression expression);
        void VisitIdentifierExpression(IdentifierExpression expression);
        void VisitIfStatementExpression(IfStatementExpression expression);
        void VisitForStatementExpression(ForStatementExpression expression);
        void VisitWhileStatementExpression(WhileStatementExpression expression);
        void VisitDoWhileStatementExpression(DoWhileStatementExpression expression);
        void VisitReturnExpression(ReturnExpression expression);
    }
}
