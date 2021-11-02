using Lexing;
using System.Diagnostics;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class ImportStatementExpression : ExpressionBase
    {
        public ImportStatementExpression(string path, string? alias, string? filename) : base(ExpressionType.DontCare)
        {
            Path = path;
            Alias = alias;
            ImportedInFile = filename;
        }

        public string Path { get; }
        public string? Alias { get; }
        public string? ImportedInFile { get; }
    }
}
