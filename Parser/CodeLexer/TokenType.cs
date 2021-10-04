namespace Parser.CodeLexer
{
    internal enum TokenType
    {
        ToDo = -1337,               // should be removed or repurposed...
        Error = -2,

        EndOfFile = -1,
        Undefined,

        Number,
        Float,
        Double,
        Integer,
        Character,
        String,
        DateTime,
        Hexadecimal,

        AccoladesOpen,              // {
        AccoladesClose,             // }
        BracketsOpen,               // [
        BracketsClose,              // ]
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
        While,                      // While
        Do,                         // While... Do..
        For,                        // For
        ForEach,                    // Foreach
        In,                         // Foreach x in [x,x,x]

        FunctionName,
        Identifier,
        Letters,
        EndOfStatement,             // ;
        TerniaryOperatorTrue,       // ?
        TerniaryOperatorFalse,      // :
        Add,                       // +
        Subtract,                      // -
        Multiply,                      // *
        Divide,                     // /
        Modulo,                     // %
        NullableCoalesce,           // ??
        AddAssign,             // +=
        SubtractAssign,            // -=
        MultiplyAssign,            // *=
        DivideAssign,           // /=
        ModuloAssignment,           // %=
        NullableCoalesceAssignment, // ??=
        BooleanInvert,              // !
        Summary,                    ///        
        Comment,                    //
        VariableDeclaration,       // var, auto
        PublicScope,                // public
        PrivateScope,               // private
        InternalScope,              // internal
        ProtectedScope,             // protected
        FunctionDefinition,
        ReAssignment,
    }
}
