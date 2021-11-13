using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class ValueExpression : ExpressionBase
    {
        public ValueExpression(Token token) : base(token, ExpressionType.Value)
        {
            // todo: how to handle nullables?
            if(token?.Value == null)
            {
                throw new ArgumentException("Provided token must have a NON NULL value!");
            }
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
        public object Value { get => Token.Value; set => Token.Value = value; }
        public TypeIndicator TypeIndicator { get => Token.TypeIndicator; set => Token.TypeIndicator = value; }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8603 // Possible null reference return.


    }
}
