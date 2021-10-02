using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.AbstractSyntaxTree
{
    internal abstract class ExpressionVisitor
    {
        protected ExpressionVisitor()
        {
        }

        public virtual ExpressionAST Visit(ExpressionAST node)
        {
            if (node != null)
            {
                return node.Accept(this);
            }

            return null;
        }

        protected internal virtual ExpressionAST VisitExtension(ExpressionAST node)
        {
            return node.VisitChildren(this);
        }

        protected internal virtual ExpressionAST VisitBinaryExpressionAST(BinaryExpressionAST node)
        {
            this.Visit(node.Lhs);
            this.Visit(node.Rhs);

            return node;
        }

        protected internal virtual ExpressionAST VisitCallExpressionAST(CallExpressionAST node)
        {
            foreach (var argument in node.Arguments)
            {
                this.Visit(argument);
            }

            return node;
        }

        protected internal virtual ExpressionAST VisitFunctionAST(FunctionAST node)
        {
            this.Visit(node.Proto);
            this.Visit(node.Body);

            return node;
        }

        protected internal virtual ExpressionAST VisitVariableExpressionAST(VariableExpressionAST node)
        {
            return node;
        }

        protected internal virtual ExpressionAST VisitPrototypeAST(PrototypeAST node)
        {
            return node;
        }

        protected internal virtual ExpressionAST VisitNumberExpressionAST(NumberExpressionAST node)
        {
            return node;
        }
    }
}
