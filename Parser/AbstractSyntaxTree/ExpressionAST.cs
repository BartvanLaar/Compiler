using Compiler;
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
        FunctionCall,
        Number,
        DivideRest,
        GreaterThan,
        Equivalent,
        Equals,
        GreaterThanEqual,
        LessThanEqual
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
        public NumberExpressionAST(double value) : base(ExpressionType.Number)
        {
            Value = value;
        }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitNumberExpressionAST(this);
        }
    }

    internal sealed class VariableEvaluationExpressionAST : ExpressionAST
    {
        public VariableEvaluationExpressionAST(string variableName) : base(ExpressionType.VariableEvaluation)
        {
            VariableName = variableName;
        }

        public string VariableName { get; }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitVariableEvaluationExpressionAST(this);
        }
    }

    internal sealed class BinaryExpressionAST : ExpressionAST
    {
        public BinaryExpressionAST(string @operator, ExpressionAST leftHandSide, ExpressionAST rightHandSide) : base(DetermineExpressionType(@operator))
        {
            LeftHandSide = leftHandSide;
            RightHandSide = rightHandSide;
        }

        public ExpressionAST LeftHandSide { get; }
        public ExpressionAST RightHandSide { get; }

        private static ExpressionType DetermineExpressionType(string @operator)
        {
            switch (@operator)
            {
                case LexerConstants.PLUS_SIGN:
                    return ExpressionType.Add;
                case LexerConstants.MINUS_SIGN:
                    return ExpressionType.Subtract;
                case LexerConstants.TIMES_SIGN:
                    return ExpressionType.Multiply;
                case LexerConstants.DIVIDE_SIGN:
                    return ExpressionType.Divide;
                case LexerConstants.MODULO_SIGN:
                    return ExpressionType.DivideRest;
                case LexerConstants.GREATER_THAN_SIGN:
                    return ExpressionType.GreaterThan;
                case LexerConstants.GREATER_THAN_EQUAL_SIGN:
                    return ExpressionType.GreaterThanEqual;
                case LexerConstants.LESS_THAN_SIGN:
                    return ExpressionType.LessThan;
                case LexerConstants.LESS_THAN_EQUAL_SIGN:
                    return ExpressionType.LessThanEqual;
                case LexerConstants.EQUIVALENT_SIGN:
                    return ExpressionType.Equivalent;
                case LexerConstants.EQUALS_SIGN:
                    return ExpressionType.Equals;

                default:
                    throw new ArgumentException($"Operator {@operator} is not supported.");
            }
        }

    }
    internal sealed class MethodCallExpressionAST : ExpressionAST
    {
        public MethodCallExpressionAST(string callee, ExpressionAST[] methodArguments) : base(ExpressionType.MethodCall)
        {
            Callee = callee;
            MethodArguments = methodArguments;
        }

        public string Callee { get; }
        public ExpressionAST[] MethodArguments { get; }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitMethodCallExpressionAST(this);
        }
    }
    internal sealed class PrototypeAST : ExpressionAST
    {
        public PrototypeAST(string name, string[] arguments) : base(ExpressionType.Prototype)
        {
            Name = name;
            Arguments = arguments;
        }

        public string Name { get; }
        public string[] Arguments { get; }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitPrototypeAST(this);
        }
    }

    internal sealed class FunctionCallExpressionAST : ExpressionAST
    {
        public FunctionCallExpressionAST(PrototypeAST prototype, ExpressionAST body) : base(ExpressionType.FunctionCall)
        {
            Prototype = prototype;
            Body = body;
        }

        public PrototypeAST Prototype { get; }
        public ExpressionAST Body { get; }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitFunctionCallExpressionAST(this);
        }
    }
}
