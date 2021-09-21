using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    internal interface ICodeGenerator
    {
        Task<ICodeGeneratorResults> GenerateAsync(IAttributeEvaluatorResults attributeEvaluatorResults);
    }
    internal interface ICodeGeneratorResults
    {

    }

    internal class CodeGeneratorResults : ICodeGeneratorResults
    {

    }

    internal class CodeGenerator : ICodeGenerator
    {
        public async Task<ICodeGeneratorResults> GenerateAsync(IAttributeEvaluatorResults attributeEvaluatorResults)
        {

            var results = new CodeGeneratorResults();
            return results;
        }
    }
}
