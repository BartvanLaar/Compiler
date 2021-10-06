
namespace Parser.AbstractSyntaxTree.Expressions
{
    internal enum ExpressionType
    {

        Add,
        Subtract,
        Multiply,
        Divide,
        LessThan,
        MethodCall,
        Identifier,
        Prototype,
        FunctionCall,
        Double,
        DivideRest,
        GreaterThan,
        Equivalent,
        Equals,
        GreaterThanEqual,
        LessThanEqual,
        Float,
        Integer,
        String,
        Character,
        Assignment,
        [Obsolete("Is this even useful?")]
        Body,
        IfStatementExpression,
        ForStatementExpression
    }

}
