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

        protected internal virtual ExpressionBase VisitBinaryExpression(BinaryExpression node)
        {
            Visit(node.LeftHandSide);
            Visit(node.RightHandSide);

            return node;
        }

        protected internal virtual ExpressionBase VisitMethodCallExpression(MethodCallExpression node)
        {
            foreach (var argument in node.MethodArguments)
            {
                Visit(argument);
            }

            return node;
        }
        
        // Why does this exist? Whats the difference between a Function call and a method call.. ?
        protected internal virtual ExpressionBase VisitFunctionCallExpression(FunctionCallExpression node)
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

        protected internal virtual ExpressionBase VisitVariableEvaluationExpression(VariableEvaluationExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitPrototype(PrototypeExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitDoubleExpression(DoubleExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitFloatExpression(FloatExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitIntegerExpression(IntegerExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitStringExpression(StringExpression node)
        {
            return node;
        }

        protected internal virtual ExpressionBase VisitCharacterExpression(CharacterExpression node)
        {
            return node;
        }
    }
}
