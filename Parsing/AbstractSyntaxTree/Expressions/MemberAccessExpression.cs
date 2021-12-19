using Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override ExpressionType DISCRIMINATOR => ExpressionType.MemberAccess;
    }
}
