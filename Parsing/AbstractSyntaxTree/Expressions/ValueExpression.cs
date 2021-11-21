using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public sealed class ValueExpression : ValueExpressionBase
    {
        public ValueExpression(Token token, Token typeToken) : base(token, ExpressionType.Value)
        {
            // todo: how to handle nullables?
            if(token?.Value == null)
            {
                throw new ArgumentException("Provided token must have a NON NULL value!");
            }

            TypeToken = typeToken;
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
        public object Value { get => Token.Value; set => Token.Value = value; }
        public Token TypeToken { get; set; }
        public Token ValueToken { get => Token; }
        public TypeIndicator TypeIndicator { get => Token.TypeIndicator; set => Token.TypeIndicator = value; }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8603 // Possible null reference return.


    }
}
