﻿namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class ForStatementExpression : ExpressionBase
    {
        public ForStatementExpression(string variableName, ExpressionBase start, ExpressionBase end, ExpressionBase step, BodyExpression actionBody) : base(ExpressionType.ForStatementExpression)
        {
            VariableName = variableName;
            Start = start;
            End = end;
            Step = step;
            Body = actionBody;
        }

        public string VariableName { get; }
        public ExpressionBase Start { get; }
        public ExpressionBase End { get; }
        public ExpressionBase Step { get; }
        public ExpressionBase Body { get; }
    }
}
