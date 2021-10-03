using Parser.CodeLexer;

namespace Parser.AbstractSyntaxTree.Expressions
{
    internal sealed class VariableEvaluationExpression : ExpressionBase
    {
        public VariableEvaluationExpression(Token token) : base(token, ExpressionType.VariableEvaluation)
        {
            VariableName = token.Name;
        }

        public string VariableName { get; }

        protected internal override ExpressionBase Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitVariableEvaluationExpression(this);
        }
    }
}
