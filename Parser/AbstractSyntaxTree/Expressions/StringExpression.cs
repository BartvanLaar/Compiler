using Compiler;
using Parser.Lexer;
using System.Diagnostics;

namespace Parser.AbstractSyntaxTree.Expressions
{
    internal sealed class StringExpression : ExpressionBase
    {
        public StringExpression(Token token) : base(token, ExpressionType.String)
        {
            // todo: how to handle nullables?
            Debug.Assert(token.StringValue != null);
            Value = token.StringValue;
        }

        public string Value { get; }

        protected internal override ExpressionBase Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitStringExpressionAST(this);
        }
    }
}
