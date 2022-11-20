using Lexing;

namespace Parsing.AbstractSyntaxTree.Expressions
{
    public class ImportStatementExpression : ExpressionBase
    {
        public ImportStatementExpression(Token importToken, string path, string? alias, string? filename) : base(importToken)
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
