using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class MemberAccessExpression : ValueExpressionBase
    {
        public MemberAccessExpression(Token parentToken, ValueExpressionBase memberAccess) : base(memberAccess.Token, memberAccess.Token)
        {
            ParentToken = parentToken;
            MemberAccess = memberAccess;
        }

        public Token ParentToken { get; }
        public ValueExpressionBase MemberAccess { get; }
        public Token MemberToken => Token;

    }
}
