namespace Lexing
{
    public enum TokenType
    {
        EndOfFile,                  // EOF
        Type,                       // type token
        Value,                      // value token
        AccoladesOpen,              // {
        AccoladesClose,             // }
        BracketOpen,                // [
        BracketClose,               // ]
        ParanthesesOpen,            // (
        ParanthesesClose,           // )
        Assignment,                 // =
        Equivalent,                 // ==
        NotEquivalent,              // !=
        Equals,                     // ===
        NotEquals,                  // !==
        GreaterThan,                // >
        GreaterThanEqual,           // >=
        LessThan,                   // <
        LessThanEqual,              // <=

        BitwiseAnd,                 // &
        ConditionalAnd,             // &&
        BitwiseOr,                  // |
        ConditionalOr,              // ||
        ConditionalXOr,             // ^

        BitShiftLeft,               // <<
        BitShiftRight,              // >>>

        If,                         // If
        Else,                       // Else
        While,                      // While
        Do,                         // While... Do..
        For,                        // For
        ForEach,                    // Foreach
        In,                         // Foreach x in [x,x,x]
        Continue,                   // continue
        Break,                      // break

        VariableIdentifier,         // identifier of a variable
        FunctionIdentifier,         // identifier of a function
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
        //VariableDeclaration,        // var, auto
        FunctionDefinition,         // func
        ReturnTypeIndicator,        // ->
        ReAssignment,               // @UsedByParser @Hack @PlsRefactor...
        ArgumentSeparator,          // ,
        Definition,
        Return,

        Export,                     // export
        Extend,                     // extend
        Extern,                     // extern

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
        As,
        Constant,
        Static,
        Interface,
        Class,
        Enum,
        Array,
        Namespace,
        Dot,
    }
}
