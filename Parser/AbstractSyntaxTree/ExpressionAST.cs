using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.AbstractSyntaxTree
{
    internal enum ExpressionType
    {

        Add,
        Subtract,
        Multiply,
        Divide,
        LessThan,
        MethodCall,
        VariableEvaluation,
        Prototype,
        Function,
        Number
    }

    internal abstract class ExpressionAST
    {

        public ExpressionAST(ExpressionType expressionType)
        {
            NodeExpressionType = expressionType;
        }
        public ExpressionType NodeExpressionType { get; }

        protected internal virtual ExpressionAST VisitChildren(ExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }

        protected internal virtual ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitExtension(this);
        }
    }

    internal sealed class NumberExpressionAST : ExpressionAST
    {
        public double Value { get; }
        public NumberExpressionAST(double value) : base (ExpressionType.Number)
        {
            Value = value;
        }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitNumberExpressionAST(this);
        }
    }

}
