namespace Lexing
{
    public enum TokenType
    {
        Error,
        EndOfFile,

        Float,
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

        Also,                       // &
        AndAlso,                    // &&
        Or,                         // |
        OrElse,                     // ||
        xOr,                        // ^
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
        Letters,
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
        Power,                      // ^
        NullableCoalesce,           // ??
        AddAssign,                  // +=
        SubtractAssign,             // -=
        MultiplyAssign,             // *=
        DivideAssign,               // /=
        NullableCoalesceAssign, // ??=
        BooleanInvert,              // !
        Summary,                    ///        
        Comment,                    //
        VariableDeclaration,        // var, auto
        FunctionDefinition,         // func
        ReturnTypeIndicator,        // ->
        ReAssignment,               // @UsedByParser @Hack @PlsRefactor...
        ArgumentSeparator,          // ,
        Definition,
        Return,

        Export,                     // export
        Extend,                     // extend
        Extern,                     // extern

        True,
        False,
        Null,
        Params,
        Is,
        Not,
        And,
        ImportStatement,
    }
}
