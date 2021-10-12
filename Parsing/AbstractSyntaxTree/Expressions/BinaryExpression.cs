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
                LexerConstants.MINUS => ExpressionType.Subtract,
                LexerConstants.TIMES => ExpressionType.Multiply,
                LexerConstants.DIVIDE => ExpressionType.Divide,
                LexerConstants.MODULO => ExpressionType.DivideRest,
                LexerConstants.GREATER_THAN_SIGN => ExpressionType.GreaterThan,
                LexerConstants.GREATER_THAN_EQUAL_SIGN => ExpressionType.GreaterThanEqual,
                LexerConstants.LESS_THAN_SIGN => ExpressionType.LessThan,
                LexerConstants.LESS_THAN_EQUAL_SIGN => ExpressionType.LessThanEqual,
                LexerConstants.EQUIVALENT_SIGN => ExpressionType.Equivalent,
                LexerConstants.EQUALS_SIGN => ExpressionType.Equals,
                LexerConstants.OR => ExpressionType.Or,
                LexerConstants.OR_ELSE => ExpressionType.OrElse,
                LexerConstants.AND => ExpressionType.And,
                LexerConstants.AND_ALSO => ExpressionType.AndAlso,
                _ => throw new ArgumentException($"Operator {@operator} is not supported."),
            };
        }

    }
}
