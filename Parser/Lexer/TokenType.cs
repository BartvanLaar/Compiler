using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Lexer
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
        VariableType,
        Letters,
        EndOfStatement,             // ;
        TerniaryOperatorTrue,       // ?
        TerniaryOperatorFalse,      // :
        Plus,                       // +
        Minus,                      // -
        Times,                      // *
        Divide,                     // /
        Modulo,                     // %
        NullableCoalesce,           // ??
        PlusAssignment,             // +=
        MinusAssignment,            // -=
        TimesAssignment,            // *=
        DivideAssignment,           // /=
        ModuloAssignment,           // %=
        NullableCoalesceAssignment, // ??=
        BooleanInvert,              // !
        Summary,                    ///        
        Comment,                    //
        VariableTypeInferred,       // var
        PublicScope,                // public
        PrivateScope,               // private
        InternalScope,              // internal
        ProtectedScope,             // protected

    }
}
