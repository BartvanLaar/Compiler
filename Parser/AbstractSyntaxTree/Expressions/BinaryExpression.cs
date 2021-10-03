using Compiler;
using Parser.Lexer;

namespace Parser.AbstractSyntaxTree.Expressions
{
    internal sealed class BinaryExpression : ExpressionBase
    {
        public BinaryExpression(Token token, ExpressionBase leftHandSide, ExpressionBase rightHandSide) : base(token, DetermineExpressionType(token.Name))
        {
            LeftHandSide = leftHandSide;
            RightHandSide = rightHandSide;
        }

        public ExpressionBase LeftHandSide { get; }
        public ExpressionBase RightHandSide { get; }

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
}
