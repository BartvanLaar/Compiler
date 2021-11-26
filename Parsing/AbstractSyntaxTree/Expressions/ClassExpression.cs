using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    internal class ClassExpression : ExpressionBase
    {
        public ClassExpression(Token token) : base(token)
        {
        }

        public override ExpressionType DISCRIMINATOR => ExpressionType.Class;
    }
}
