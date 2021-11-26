using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class BinaryExpression : ValueExpressionBase
    {
        public BinaryExpression(Token token, ValueExpressionBase leftHandSide, ValueExpressionBase? rightHandSide) : base(token, leftHandSide.Token)
        {
            LeftHandSide = leftHandSide;
            RightHandSide = rightHandSide;
        }

        public ValueExpressionBase LeftHandSide { get; }
        public ValueExpressionBase? RightHandSide { get; }

        public override ExpressionType DISCRIMINATOR => ExpressionType.Binary;
    }
}
