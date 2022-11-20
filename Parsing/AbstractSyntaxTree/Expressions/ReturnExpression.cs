using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class ReturnExpression : ExpressionBase
    {
        public ReturnExpression(Token returnToken, ExpressionBase? returnExpression, TypeIndicator functionReturnTypeIndicator) : base(returnToken)
        {
            ReturnExpr = returnExpression;
            FunctionReturnTypeIndicator = functionReturnTypeIndicator;
        }

        public ExpressionBase? ReturnExpr { get; }
        public TypeIndicator FunctionReturnTypeIndicator { get; }

    }
}
