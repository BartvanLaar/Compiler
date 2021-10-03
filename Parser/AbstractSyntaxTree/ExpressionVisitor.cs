using Parser.AbstractSyntaxTree.Expressions;

namespace Parser.AbstractSyntaxTree
{
    internal abstract class ExpressionVisitor
    {
        protected ExpressionVisitor()
        {
        }

        public virtual ExpressionBase? Visit(ExpressionBase? node)
        {
            return node?.Accept(this);
        }

        protected internal virtual ExpressionBase? VisitExtension(ExpressionBase node)
        {
            return node.VisitChildren(this);
        }

        protected internal virtual ExpressionBase VisitBinaryExpressionAST(BinaryExpression node)
        {
            Visit(node.LeftHandSide);
            Visit(node.RightHandSide);

            return node;
        }

        protected internal virtual ExpressionBase VisitMethodCallExpressionAST(MethodCallExpression node)
        {
            foreach (var argument in node.MethodArguments)
            {
                Visit(argument);
            }

            return node;
        }

        protected internal virtual ExpressionBase VisitFunctionCallExpressionAST(FunctionCallExpression node)
        {
            Visit(node.Prototype);
            Visit(node.Body);

            return node;
        }

        protected internal virtual ExpressionBase? VisitAssignmentExpression(AssignmentExpression node)
        {
            Visit(node.IdentificationExpression);
            Visit(node.ValueExpression);

            return node;
        }

        protected internal virtual ExpressionBase VisitVariableEvaluationExpressionAST(VariableEvaluationExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitPrototypeAST(PrototypeExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitDoubleExpressionAST(DoubleExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitFloatExpressionAST(FloatExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitIntegerExpressionAST(IntegerExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitStringExpressionAST(StringExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitCharacterExpressionAST(CharacterExpression node)
        {
            return node;
        }
    }
}
