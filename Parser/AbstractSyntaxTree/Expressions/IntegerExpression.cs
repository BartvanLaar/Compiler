using Compiler;
using Parser.Lexer;
using System.Diagnostics;

namespace Parser.AbstractSyntaxTree.Expressions
{
    internal sealed class IntegerExpression : ExpressionBase
    {
        public IntegerExpression(Token token) : base(token, ExpressionType.Integer)
        {
            // todo: how to handle nullables?
            Debug.Assert(token.IntegerValue.HasValue);
            Value = token.IntegerValue.Value;
        }
        public int Value { get; }

        protected internal override ExpressionBase Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitIntegerExpressionAST(this);
        }
    }
}
