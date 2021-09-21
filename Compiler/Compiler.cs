namespace Compiler
{
    
    public static class Compiler
    { 
        public static async Task CompileFileAsync(IEnumerable<string> filesToCompile)
        {
            if(filesToCompile.Any(f => !File.Exists(f)))
            {
                throw new InvalidOperationException("Can't seem to find the provided files...");
            }
           await CompileAsync(await Task.WhenAll(filesToCompile.Select(f => File.ReadAllTextAsync(f))));
        }

        public static async Task CompileFileAsync(string fileToCompile)
        {
            if (!File.Exists(fileToCompile))
            {
                throw new InvalidOperationException("Can't seem to find the provided file...");
            }

            await CompileAsync(new[] { await File.ReadAllTextAsync(fileToCompile) });
        }

        public static Task CompileTextAsync(string textToCompile)
        {
            if(File.Exists(textToCompile))
            {
                throw new InvalidOperationException($"Please call {nameof(CompileFileAsync)} instead.");
            }

            return CompileAsync(new[] { textToCompile });
        }

        private static async Task CompileAsync(string[] textFiles)
        {
            var lexer = new Lexer();
            var parser = new Parser();
            var attributeEvaluator = new AttributeEvaluator();
            var codeGenerator = new CodeGenerator();

            var lexerResults = await lexer.LexAsync(textFiles);
            var parserResults = await parser.ParseAsync(lexerResults);
            var attributeEvaluatorResults = await attributeEvaluator.EvaluateAsync(parserResults);

            var codeGenerationResults = await codeGenerator.GenerateAsync(attributeEvaluatorResults);
            //todo: what to do with output?
            //return codeGenerationResults;
        }
    }
}