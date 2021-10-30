namespace Lexing
{
    public enum TokenType
    {
        Error,
        EndOfFile,

        Float, // should return types be their own enum type? and replaced as a token called ReturnType?
        Double,
        Boolean,
        Integer,
        Character,
        String,
        DateTime, // !notimplemented
        Void,
        Hexadecimal,

        AccoladesOpen,              // {
        AccoladesClose,             // }
        BracketOpen,               // [
        BracketClose,              // ]
        ParanthesesOpen,            // (
        ParanthesesClose,           // )
        Assignment,                 // =
        Equivalent,                 // ==
        NotEquivalent,              // !=
        Equals,                     // ===
        NotEquals,                  // !==
        GreaterThan,                // >
        GreaterThanOrEqualTo,       // >=
        LessThan,                   // <
        LessThanOrEqualTo,          // <=

        LogicalAnd,                       // &
        ConditionalAnd,                    // &&
        LogicalOr,                         // |
        ConditionalOr,                     // ||
        LogicalXOr,                        // ^

        BitShiftLeft,               // <<
        BitShiftRight,              // >>>

        If,                         // If
        Else,                       // Else
        While,                      // While
        Do,                         // While... Do..
        For,                        // For
        ForEach,                    // Foreach
        In,                         // Foreach x in [x,x,x]
        Continue,
        Break,

        FunctionName,
        Identifier,
        EndOfStatement,             // ;
        TerniaryOperatorTrue,       // ?
        TerniaryOperatorFalse,      // :
        Add,                        // +
        AddAdd,                     // ++
        Subtract,                   // -
        SubtractSubtract,           // --
        Multiply,                   // *
        Divide,                     // /
        Modulo,                     // %
        //Power,                      // ^ //todo: dit moet anders geimplementeerd, misschien middels ** ?
        NullableCoalesce,           // ??
        AddAssign,                  // +=
        SubtractAssign,             // -=
        MultiplyAssign,             // *=
        DivideAssign,               // /=
        NullableCoalesceAssign,     // ??=
        BooleanInvert,              // !
        Summary,                    ///        
        Comment,                    //
        VariableDeclaration,        // var, auto
        FunctionDefinition,         // func
        ReturnTypeIndicator,        // ->
        ReAssignment,               // @UsedByParser @Hack @PlsRefactor...
        ArgumentSeparator,          // ,
        Definition,
        ReturnStatement,

        Export,                     // export
        Extend,                     // extend
        Extern,                     // extern

        True,                       // should this be tokentype.Keyword and then their own enum? or just only strings? Now its kind of double administration
        False,
        Null,
        Params,
        Is,
        Not,
        ImportStatement,
        ModuloAssign,
        LogicalXOrAssign,
        LogicalAndAssign,
        LogicalOrAssign,
        BitShiftLeftAssign,
        BitShiftRightAssign,
    }
}
