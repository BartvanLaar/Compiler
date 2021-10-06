using Parser.AbstractSyntaxTree.Expressions;

namespace Parser.AbstractSyntaxTree.Visitors
{
    public interface IAbstractSyntaxTreeVisitor
    {
        void VisitIntegerExpression(IntegerExpression expression);
        void VisitDoubleExpression(DoubleExpression expression);
        void VisitFloatExpression(FloatExpression expression);
        void VisitStringExpression(StringExpression expression);
        void VisitCharacterExpression(CharacterExpression expression);
        void VisitBinaryExpression(BinaryExpression expression);
        void VisitPrototypeExpression(PrototypeExpression expression);
        void VisitMethodCallExpression(MethodCallExpression expression);
        void VisitFunctionCallExpression(FunctionCallExpression expression);
        void VisitAssignmentExpression(AssignmentExpression expression);
        void VisitIdentifierExpression(IdentifierExpression expression);
        [Obsolete("Is this one even required?")]
        void VisitBodyExpression(BodyExpression expression);
        void VisitIfStatementExpression(IfStatementExpression expression);
        void VisitForStatementExpression(ForStatementExpression expression);

    }
}
