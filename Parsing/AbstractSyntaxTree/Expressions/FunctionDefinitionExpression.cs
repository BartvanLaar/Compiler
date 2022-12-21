using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{

    public sealed class FunctionDefinitionExpression : ExpressionBase
    {
        public FunctionDefinitionExpression(Token identifierToken, DefinitionArgument[] arguments, Token returnTypeToken, BodyExpression? functionBody, bool isExtern, bool isExport) : base(identifierToken)
        {
            Arguments = arguments;
            ReturnTypeToken = returnTypeToken;
            Body = functionBody;
            IsExtern = isExtern;
            IsExport = isExport;
        }

        public string FunctionName { get => Token.Name; set => Token.Name = value; }
        public DefinitionArgument[] Arguments { get; }
        public Token ReturnTypeToken { get; }
        public BodyExpression? Body { get; }
        public bool IsExtern { get; }
        public bool IsExport { get; }

    }

    public class DefinitionArgument
    {
        public DefinitionArgument(Token typeToken, Token valueToken)
        {
            TypeToken = typeToken;
            ValueToken = valueToken;
        }

        public Token TypeToken { get; }
        public Token ValueToken { get; }
    }

}
