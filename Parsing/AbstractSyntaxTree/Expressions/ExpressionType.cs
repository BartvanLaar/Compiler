namespace Parsing.AbstractSyntaxTree.Expressions
{
    public enum ExpressionType
    {
        DontCare = -1,
        Add,
        Subtract,
        Multiply,
        Divide,
        LessThan,
        FunctionCall,
        Identifier,
        Double,
        DivideRest,
        GreaterThan,
        Equivalent,
        Equals,
        NotEquivalent,
        NotEquals,
        GreaterThanEqual,
        LessThanEqual,
        BitShiftLeft,
        BitShiftRight,
        Float,
        Integer,
        String,
        Character,
        Assignment,
        Body,
        IfStatementExpression,
        ForStatementExpression,
        BooleanValue,
        WhileStatementExpression,
        Or,
        OrElse,
        And,
        AndAlso,
        FunctionDefinition,

    }

}
