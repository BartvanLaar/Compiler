using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class ImportStatementExpression : ExpressionBase
    {
        public ImportStatementExpression(Token  token) : base(token, ExpressionType.DontCare)
        {
            Debug.Assert(token.StringValue is not null);
            Path = token.StringValue;
        }

        public string Path { get; set; }
    }
}
