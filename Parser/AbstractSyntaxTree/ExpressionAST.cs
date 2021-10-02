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
        Double,
        DivideRest,
        GreaterThan,
        Equivalent,
        Equals,
        GreaterThanEqual,
        LessThanEqual,
        Float,
        Integer,
        String,
        Character
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

    internal sealed class DoubleExpressionAST : ExpressionAST
    {
        public double Value { get; }
        public DoubleExpressionAST(double value) : base(ExpressionType.Double)
        {
            Value = value;
        }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitDoubleExpressionAST(this);
        }
    }

    internal sealed class FloatExpressionAST : ExpressionAST
    {
        public FloatExpressionAST(float value): base(ExpressionType.Float)
        {
            Value = value;
        }
        public float Value {  get; }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitFloatExpressionAST(this);
        }
    }

    internal sealed class IntegerExpressionAST : ExpressionAST
    {
        public IntegerExpressionAST(int value) : base(ExpressionType.Integer)
        {
            Value = value;
        }
        public int Value { get; }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitIntegerExpressionAST(this);
        }
    }

    internal sealed class StringExpressionAST : ExpressionAST
    {
        public StringExpressionAST(string value) : base(ExpressionType.String)
        {
            Value = value;
        }

        public string Value { get; }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitStringExpressionAST(this);
        }
    }

    internal sealed class CharacterExpressionAST : ExpressionAST
    {
        public CharacterExpressionAST(char value) : base(ExpressionType.Character)
        {
            Value = value;
        }

        public char Value { get; }

        protected internal override ExpressionAST Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitCharacterExpressionAST(this);
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
            return @operator switch
            {
                LexerConstants.PLUS_SIGN => ExpressionType.Add,
                LexerConstants.MINUS_SIGN => ExpressionType.Subtract,
                LexerConstants.TIMES_SIGN => ExpressionType.Multiply,
                LexerConstants.DIVIDE_SIGN => ExpressionType.Divide,
                LexerConstants.MODULO_SIGN => ExpressionType.DivideRest,
                LexerConstants.GREATER_THAN_SIGN => ExpressionType.GreaterThan,
                LexerConstants.GREATER_THAN_EQUAL_SIGN => ExpressionType.GreaterThanEqual,
                LexerConstants.LESS_THAN_SIGN => ExpressionType.LessThan,
                LexerConstants.LESS_THAN_EQUAL_SIGN => ExpressionType.LessThanEqual,
                LexerConstants.EQUIVALENT_SIGN => ExpressionType.Equivalent,
                LexerConstants.EQUALS_SIGN => ExpressionType.Equals,
                _ => throw new ArgumentException($"Operator {@operator} is not supported."),
            };
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
