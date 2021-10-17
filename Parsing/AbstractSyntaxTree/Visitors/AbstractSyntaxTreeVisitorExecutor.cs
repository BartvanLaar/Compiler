using Parsing.AbstractSyntaxTree.Expressions;

namespace Parsing.AbstractSyntaxTree.Visitors
{
    public class AbstractSyntaxTreeVisitorExecutor : IAbstractSyntaxTreeVisitorExecuter
    {
        private IByteCodeGeneratorListener? _listener;

        public string Execute(Queue<ExpressionBase> abstractSyntaxTrees, IByteCodeGeneratorListener listener)
        {
            _listener = listener;

            while (abstractSyntaxTrees.Any())
            {
                Visit(abstractSyntaxTrees.Dequeue());
            }

            var resultFilePath = string.Empty;
            return resultFilePath; // for now support a single file.
        }

        public void Visit(ExpressionBase? expression)
        {
            if (expression == null)
            {
                return;
            }

            switch (expression.NodeExpressionType)
            {
                // These are all handled by binary operator expressions.
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    VisitBinaryExpression((BinaryExpression)expression);
                    break;
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.DivideRest:
                    VisitBinaryExpression((BinaryExpression)expression);
                    break;
                // These are all handled by binary operator expressions.
                case ExpressionType.Equivalent:
                case ExpressionType.Equals:
                case ExpressionType.NotEquivalent:
                case ExpressionType.NotEquals:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanEqual:
                    VisitBinaryExpression((BinaryExpression)expression);
                    break;

                case ExpressionType.FunctionCall:
                    VisitFunctionCallExpression((FunctionCallExpression)expression);
                    break;
                case ExpressionType.Identifier:
                    VisitIdentifierExpression((IdentifierExpression)expression);
                    break;
                case ExpressionType.FunctionDefinition:
                    VisitFunctionDefinitionExpression((FunctionDefinitionExpression)expression);
                    break;
                case ExpressionType.Double:
                    VisitDoubleExpression((DoubleExpression)expression);
                    break;
                case ExpressionType.Float:
                    VisitFloatExpression((FloatExpression)expression);
                    break;
                case ExpressionType.Integer:
                    VisitIntegerExpression((IntegerExpression)expression);
                    break;
                case ExpressionType.BooleanValue:
                    VisitBooleanExpression((BooleanExpression)expression);
                    break;
                case ExpressionType.String:
                    VisitStringExpression((StringExpression)expression);
                    break;
                case ExpressionType.Character:
                    VisitCharacterExpression((CharacterExpression)expression);
                    break;
                case ExpressionType.Assignment:
                    VisitAssignmentExpression((AssignmentExpression)expression);
                    break;
                case ExpressionType.IfStatementExpression:
                    VisitIfStatementExpression((IfStatementExpression)expression);
                    break;
                case ExpressionType.WhileStatementExpression:
                    VisitWhileStatementExpression((WhileStatementExpression)expression);
                    break;
                //case ExpressionType.ForStatementExpression:
                //    VisitForStatementExpression((ForStatementExpression)expression);
                //    break;
                default:
                    // should this be visiting a top level?
                    throw new ArgumentException($"Unknown expression type encountered: '{expression.NodeExpressionType}'");
            }
        }


        public void VisitWhileStatementExpression(WhileStatementExpression expression)
        {
            Visit(expression.WhileCondition);
            Visit(expression.DoBody);

            _listener?.VisitWhileStatementExpression(expression);
        }

        public void VisitBooleanExpression(BooleanExpression expression)
        {
            _listener?.VisitBooleanExpression(expression);
        }

        public void VisitIntegerExpression(IntegerExpression expression)
        {
            _listener?.VisitIntegerExpression(expression);
        }

        public void VisitDoubleExpression(DoubleExpression expression)
        {
            _listener?.VisitDoubleExpression(expression);
        }

        public void VisitFloatExpression(FloatExpression expression)
        {
            _listener?.VisitFloatExpression(expression);
        }

        public void VisitStringExpression(StringExpression expression)
        {
            _listener?.VisitStringExpression(expression);
        }

        public void VisitCharacterExpression(CharacterExpression expression)
        {
            _listener?.VisitCharacterExpression(expression);
        }

        public void VisitBinaryExpression(BinaryExpression expression)
        {
            Visit(expression.LeftHandSide);
            Visit(expression.RightHandSide);

            _listener?.VisitBinaryExpression(expression);
        }

        public void VisitFunctionCallExpression(FunctionCallExpression expression)
        {
            foreach (var argument in expression.Arguments)
            {
                Visit(argument);
            }

            _listener?.VisitFunctionCallExpression(expression);
        }

        public void VisitFunctionDefinitionExpression(FunctionDefinitionExpression expression)
        {
            _listener?.VisitFunctionDefinitionExpression(expression);
        }

        public void VisitAssignmentExpression(AssignmentExpression expression)
        {
            Visit(expression.IdentificationExpression);
            Visit(expression.ValueExpression);

            _listener?.VisitAssignmentExpression(expression);
        }

        public void VisitIdentifierExpression(IdentifierExpression expression)
        {
            _listener?.VisitIdentifierExpression(expression);
        }

        public void VisitForStatementExpression(ForStatementExpression expression)
        {
            _listener?.VisitForStatementExpression(expression);
            throw new NotImplementedException();
        }

        public void VisitIfStatementExpression(IfStatementExpression expression)
        {
            Visit(expression.IfCondition);
            Visit(expression.IfBody);
            Visit(expression.Else);

            _listener?.VisitIfStatementExpression(expression);
        }

        public void VisitBodyStatementExpression(BodyExpression expression)
        {
            while (expression.Body.Any())
            {
                Visit(expression.Body.Dequeue());
            }

            _listener?.VisitBodyStatementExpression(expression);
        }
    }
}
