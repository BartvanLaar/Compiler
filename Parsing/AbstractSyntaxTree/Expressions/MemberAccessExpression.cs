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
        public MemberAccessExpression(Token parentToken, Token memberToken) : base(memberToken, memberToken)
        {
            ParentToken = parentToken;
        }

        public Token ParentToken { get; }
        public Token MemberToken => Token;

        public override ExpressionType DISCRIMINATOR => ExpressionType.MemberAccess;
    }
}
