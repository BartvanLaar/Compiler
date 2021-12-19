using Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class ForeachStatementExpression : ExpressionBase
    {
        public ForeachStatementExpression(Token token): base(token)
        {

        }

        public override ExpressionType DISCRIMINATOR => ExpressionType.Foreach;
    }
}
