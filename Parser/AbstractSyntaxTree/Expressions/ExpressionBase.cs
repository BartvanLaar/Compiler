using Compiler;
using Parser.Lexer;

namespace Parser.AbstractSyntaxTree.Expressions
{
    internal abstract class ExpressionBase
    {
        protected ExpressionBase(ExpressionType expressionType) : this(null, expressionType) { }

        protected ExpressionBase(Token? token, ExpressionType expressionType)
        {
            Token = token;
            NodeExpressionType = expressionType;
        }

        public Token? Token { get; }
        public ExpressionType NodeExpressionType { get; }

        protected internal virtual ExpressionBase? VisitChildren(ExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }

        protected internal virtual ExpressionBase? Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitExtension(this);
        }
    }
}
