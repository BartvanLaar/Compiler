using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class BinaryExpression : ValueExpressionBase
    {
        public BinaryExpression(Token token, ValueExpressionBase leftHandSide, ValueExpressionBase? rightHandSide) : base(token, leftHandSide.Token, DetermineExpressionType(token.TokenType))
        {
            LeftHandSide = leftHandSide;
            RightHandSide = rightHandSide;
        }

        public ValueExpressionBase LeftHandSide { get; }
        public ValueExpressionBase? RightHandSide { get; }

        // todo: put this code elsewhere...
        private static ExpressionType DetermineExpressionType(TokenType tokenType)
        {
            return tokenType switch
            {
                TokenType.Add => ExpressionType.Add,
                TokenType.AddAssign => ExpressionType.AddAssign,
                TokenType.Subtract => ExpressionType.Subtract,
                TokenType.SubtractAssign => ExpressionType.SubtractAssign,
                TokenType.Multiply => ExpressionType.Multiply,
                TokenType.MultiplyAssign => ExpressionType.MultiplyAssign,
                TokenType.Divide => ExpressionType.Divide,
                TokenType.DivideAssign => ExpressionType.DivideAssign,
                TokenType.Modulo => ExpressionType.Modulo,
                TokenType.ModuloAssign => ExpressionType.ModuloAssign,
                TokenType.GreaterThan => ExpressionType.GreaterThan,
                TokenType.GreaterThanEqual => ExpressionType.GreaterThanEqual,
                TokenType.LessThan => ExpressionType.LessThan,
                TokenType.LessThanEqual => ExpressionType.LessThanEqual,
                TokenType.Equivalent => ExpressionType.Equivalent,
                TokenType.Equals => ExpressionType.Equals,
                TokenType.NotEquivalent => ExpressionType.NotEquivalent,
                TokenType.NotEquals => ExpressionType.NotEquals,
                TokenType.BitwiseOr => ExpressionType.BitwiseOr,
                TokenType.ConditionalOr => ExpressionType.ConditionalOr,
                TokenType.BitwiseAnd => ExpressionType.BitwiseAnd,
                TokenType.ConditionalAnd => ExpressionType.ConditionalAnd,
                TokenType.ConditionalXOr => ExpressionType.ConditionalXOr,
                TokenType.BitShiftLeft => ExpressionType.BitShiftLeft,
                TokenType.BitShiftRight => ExpressionType.BitShiftRight,
                TokenType.Assignment => ExpressionType.Assignment,
                _ => throw new ArgumentException($"Token with type '{tokenType}' is not supported."),
            };
        }

    }
}
