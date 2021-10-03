using Parser.CodeLexer;
using System.Diagnostics;

namespace Parser.AbstractSyntaxTree.Expressions
{
    internal sealed class FloatExpression : ExpressionBase
    {
        public FloatExpression(Token token) : base(token, ExpressionType.Float)
        {
            // todo: how to handle nullables?
            Debug.Assert(token.FloatValue.HasValue);
            Value = token.FloatValue.Value;
        }
        public float Value { get; }

        protected internal override ExpressionBase Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitFloatExpression(this);
        }
    }
}
