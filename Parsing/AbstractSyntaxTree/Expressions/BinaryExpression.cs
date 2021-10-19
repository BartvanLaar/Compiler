using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class BinaryExpression : ExpressionBase
    {
        public BinaryExpression(Token token, ExpressionBase leftHandSide, ExpressionBase? rightHandSide) : base(token, DetermineExpressionType(token.StringValue))
        {
            LeftHandSide = leftHandSide;
            RightHandSide = rightHandSide;
        }

        public ExpressionBase LeftHandSide { get; }
        public ExpressionBase? RightHandSide { get; }

        private static ExpressionType DetermineExpressionType(string? @operator)
        {
            //todo: why not use token type here instead? its basically the same...
            Debug.Assert(@operator != null);
            return @operator switch
            {
                LexerConstants.PLUS => ExpressionType.Add,
                LexerConstants.PLUS_ASSIGN => ExpressionType.AddAssign,
                LexerConstants.MINUS => ExpressionType.Subtract,
                LexerConstants.MINUS_ASSIGN => ExpressionType.SubtractAssign,
                LexerConstants.TIMES => ExpressionType.Multiply,
                LexerConstants.TIMES_ASSIGN => ExpressionType.MultiplyAssign,
                LexerConstants.DIVIDE => ExpressionType.Divide,
                LexerConstants.DIVIDE_ASSIGN => ExpressionType.DivideAssign,
                LexerConstants.MODULO => ExpressionType.DivideRest,
                LexerConstants.MODULO_ASSIGN => ExpressionType.DivideRestAssign,
                LexerConstants.GREATER_THAN_SIGN => ExpressionType.GreaterThan,
                LexerConstants.GREATER_THAN_EQUAL_SIGN => ExpressionType.GreaterThanEqual,
                LexerConstants.LESS_THAN_SIGN => ExpressionType.LessThan,
                LexerConstants.LESS_THAN_EQUAL_SIGN => ExpressionType.LessThanEqual,
                LexerConstants.EQUIVALENT_SIGN => ExpressionType.Equivalent,
                LexerConstants.EQUALS_SIGN => ExpressionType.Equals,
                LexerConstants.NOT_EQUIVALENT_SIGN => ExpressionType.NotEquivalent,
                LexerConstants.NOT_EQUALS_SIGN => ExpressionType.NotEquals,
                LexerConstants.OR => ExpressionType.LogicalOr,
                LexerConstants.OR_ELSE => ExpressionType.ConditionalOr,
                LexerConstants.AND => ExpressionType.LogicalAnd,
                LexerConstants.AND_ALSO => ExpressionType.ConditionalAnd,
                LexerConstants.XOr => ExpressionType.LogicalXOr,
                LexerConstants.BIT_SHIFT_LEFT => ExpressionType.BitShiftLeft,
                LexerConstants.BIT_SHIFT_RIGHT => ExpressionType.BitShiftRight,
                LexerConstants.ASSIGN_OPERATOR => ExpressionType.Assignment,
                _ => throw new ArgumentException($"Operator {@operator} is not supported."),
            };
        }

    }
}
