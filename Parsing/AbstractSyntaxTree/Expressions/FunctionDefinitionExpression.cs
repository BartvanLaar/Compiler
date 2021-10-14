using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{

    public sealed class FunctionDefinitionExpression : ExpressionBase
    {
        public FunctionDefinitionExpression(Token functionIdentifierToken, FunctionDefinitionArgument[] arguments, Token returnTypeToken, BodyExpression? functionBody, bool isExtern, bool isExport) : base(functionIdentifierToken, ExpressionType.FunctionDefinition)
        {
            Arguments = arguments;
            ReturnTypeToken = returnTypeToken;
            FunctionBody = functionBody;
            IsExtern = isExtern;
            IsExport = isExport;
        }

        public FunctionDefinitionArgument[] Arguments { get; }
        public Token ReturnTypeToken { get; }
        public BodyExpression? FunctionBody { get; }
        public bool IsExtern { get; }
        public bool IsExport { get; }
    }

    public struct FunctionDefinitionArgument
    {
        public FunctionDefinitionArgument(Token typeToken, Token valueToken)
        {
            TypeToken = typeToken;
            ValueToken = valueToken;
        }

        public Token TypeToken { get; }
        public Token ValueToken { get; }
    }

}
