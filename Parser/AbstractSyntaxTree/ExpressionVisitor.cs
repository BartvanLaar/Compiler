namespace Parser.AbstractSyntaxTree
{
    internal abstract class ExpressionVisitor
    {
        protected ExpressionVisitor()
        {
        }

        public virtual ExpressionAST Visit(ExpressionAST node)
        {
            return node?.Accept(this);
        }

        protected internal virtual ExpressionAST VisitExtension(ExpressionAST node)
        {
            return node.VisitChildren(this);
        }

        protected internal virtual ExpressionAST VisitBinaryExpressionAST(BinaryExpressionAST node)
        {
            Visit(node.LeftHandSide);
            Visit(node.RightHandSide);

            return node;
        }

        protected internal virtual ExpressionAST VisitMethodCallExpressionAST(MethodCallExpressionAST node)
        {
            foreach (var argument in node.MethodArguments)
            {
                Visit(argument);
            }

            return node;
        }

        protected internal virtual ExpressionAST VisitFunctionCallExpressionAST(FunctionCallExpressionAST node)
        {
            Visit(node.Prototype);
            Visit(node.Body);

            return node;
        }

        protected internal virtual ExpressionAST VisitVariableEvaluationExpressionAST(VariableEvaluationExpressionAST node)
        {
            return node;
        }

        protected internal virtual ExpressionAST VisitPrototypeAST(PrototypeAST node)
        {
            return node;
        }

        protected internal virtual ExpressionAST VisitDoubleExpressionAST(DoubleExpressionAST node)
        {
            return node;
        }

        protected internal virtual ExpressionAST VisitFloatExpressionAST(FloatExpressionAST node)
        {
            return node;
        }

        protected internal virtual ExpressionAST VisitIntegerExpressionAST(IntegerExpressionAST node)
        {
            return node;
        }

        protected internal virtual ExpressionAST VisitStringExpressionAST(StringExpressionAST node)
        {
            return node;
        }

        protected internal virtual ExpressionAST VisitCharacterExpressionAST(CharacterExpressionAST node)
        {
            return node;
        }
    }
}
