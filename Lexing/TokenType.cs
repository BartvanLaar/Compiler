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
        NullableCoalesce,           // ??
        AddAssign,                  // +=
        SubtractAssign,             // -=
        MultiplyAssign,             // *=
        DivideAssign,               // /=
        ModuloAssignment,           // %=
        NullableCoalesceAssignment, // ??=
        BooleanInvert,              // !
        Summary,                    ///        
        Comment,                    //
        VariableDeclaration,        // var, auto
        PublicScope,                // public
        PrivateScope,               // private
        InternalScope,              // internal
        ProtectedScope,             // protected
        FunctionDefinition,         // func
        ReturnTypeIndicator,        // ->
        ReAssignment,               // @UsedByParser @Hack @PlsRefactor...
        VariableSeparator,          // ,
        Definition,
        Return,

        True,
        False,
        Null,
        Params,
        Is,
        Not,
    }
}
