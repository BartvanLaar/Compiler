using System.Collections.Generic;
using System.Linq;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    // e.g. a body of an if statement, while loop, for loop, function/method, etc.
    public sealed class BodyExpression : ExpressionBase
    {
        public BodyExpression(IEnumerable<ExpressionBase> expressions) : base(null, ExpressionType.Body)
        {
            Body = expressions as ExpressionBase[] ?? expressions.ToArray();
        }

        public ExpressionBase[] Body { get; }
    }
}
