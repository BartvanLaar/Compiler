using Lexing;
using NUnit.Framework;
using System.Linq;
using TestHelpers.Tests;

namespace Parsing.Tests
{
    internal class ParserTests
    {
        [TestCase("x + 5", 1)]
        [TestCase("x+5;", 1)]
        [TestCase("x+5;x+=5", 2)]
        [TestCase("x+5;x+=5;", 2)]
        [TestCase("x+5*5/5*5/5;", 1)]
        [TestCase("var x = 10*5;", 1)]
        [TestCase("var x = 10*(10-5);", 1)]
        [TestCase("var x = 10*(10-(5-2));", 1)]
        [TestCase("var x = (10*(10-(5-2)));", 1)]
        [TestCase("(10-1)*5;", 1)]
        public void General_Code_Test_Throw_No_Error(string code, int expectedAmountOfTrees, int expectedAmountOfErrors = 0)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);
            var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.AreEqual(expectedAmountOfTrees, ast.Count);
            Assert.AreEqual(expectedAmountOfErrors, errors.Count());
        }

        [TestCase("if(true){}", 1)]
        [TestCase("if(true){}else{}", 1)]
        [TestCase("if(true){}else if(false){}", 1)]
        [TestCase("if(true){}else if(false){} else {}", 1)]
        [TestCase("if(true){}else if(false){}else if(false){}else if(false){}else if(false){} else {}", 1)]
        [TestCase("var x = 0d;if(true){}", 2)]

        public void General_Code_Test_Throw_No_Error_If_Statements(string code, int expectedAmountOfExpressionTrees, int expectedAmountOfErrors = 0)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);
            Assert.AreEqual(expectedAmountOfExpressionTrees, ast.Count);
            Assert.AreEqual(expectedAmountOfErrors, errors.Count());
        }


        [TestCase("while(true) {}", 1)]
        [TestCase("while(true) do {}", 1)]
        [TestCase("while(true)do{}", 1)]
        [TestCase("do{} while(true)", 1)]
        [TestCase("do{}while(true)", 1)]
        public void General_Code_Test_Throw_No_Error_While_Statements(string code, int expectedAmountOfExpressionTrees, int expectedAmountOfErrors = 0)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);
            Assert.AreEqual(expectedAmountOfExpressionTrees, ast.Count);
            Assert.AreEqual(expectedAmountOfErrors, errors.Count());
        }

        //[TestCase("x--")]
        //[TestCase("x--;")]
        //[TestCase("x++")]
        //[TestCase("x++;")]
        //public void General_Code_Test_Throw_No_Error_MinusMinus_And_PlusPlus(string code)
        //{
        //    var lexer = new Lexer(code);
        //    var parser = new Parser(lexer);

        //    var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);
        //    Assert.AreEqual(1, ast.Count);
        //    Assert.IsEmpty(errors);
        //}

        //[TestCase("5++")]
        //[TestCase("5++;")]
        //[TestCase("5--")]
        //[TestCase("5--;")]
        //public void General_Code_Test_Throw_1_Error_MinusMinus_And_PlusPlus(string code)
        //{
        //    var lexer = new Lexer(code);
        //    var parser = new Parser(lexer);

        //    var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

        //    Assert.AreEqual(1, ast.Count);
        //    Assert.AreEqual(1, errors.Count());
        //}


        [TestCase("x + 5;")]
        [TestCase("x + y;")]
        [TestCase("x += 5;")]
        [TestCase("x += y;")]
        [TestCase("x - 5;")]
        [TestCase("x - y;")]
        [TestCase("x -= 5;")]
        [TestCase("x -= y;")]
        [TestCase("x * 5;")]
        [TestCase("x * y;")]
        [TestCase("x *= 5;")]
        [TestCase("x *= y;")]
        [TestCase("x / 5;")]
        [TestCase("x / y;")]
        [TestCase("x /= 5;")]
        [TestCase("x /= y;")]
        [TestCase("x %= 5;")]
        [TestCase("x %= y;")]
        [TestCase("x+5;")]
        [TestCase("x+y;")]
        [TestCase("x+=5;")]
        [TestCase("x+=y;")]
        [TestCase("x-5;")]
        [TestCase("x-y;")]
        [TestCase("x-=5;")]
        [TestCase("x-=y;")]
        [TestCase("x*5;")]
        [TestCase("x*y;")]
        [TestCase("x*=5;")]
        [TestCase("x*=y;")]
        [TestCase("x/5;")]
        [TestCase("x/y;")]
        [TestCase("x/=5;")]
        [TestCase("x/=y;")]
        [TestCase("x%=5;")]
        [TestCase("x%=y;")]
        public void Test_Valid_Parsings_No_Errors(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.AreEqual(1, ast.Count);
            Assert.IsEmpty(errors);
        }

        [TestCase("func SomeFunc() -> void {}")]
        [TestCase("extern func SomeFunc() -> void;")]
        [TestCase("extern func SomeFunc(bool x, double y, int z, string ranOutOfAlphabet) -> void;")]
        [TestCase("extern func SomeFunc() -> bool;")]
        [TestCase("extern func SomeFunc(bool x, double y, int z, string ranOutOfAlphabet) -> bool;")]
        [TestCase("extern func SomeFunc() -> int;")]
        [TestCase("extern func SomeFunc() -> double;")]
        [TestCase("export func SomeFunc() -> void {}")]
        [TestCase("export func SomeFunc() -> bool {}")]
        [TestCase("export func SomeFunc() -> double {}")]
        [TestCase("export func SomeFunc(bool x) -> void {}")]
        [TestCase("export func SomeFunc(bool x, double y, int z, string ranOutOfAlphabet) -> double {}")]
        [TestCase("export func SomeFunc(bool x, double y, int z, string ranOutOfAlphabet) -> int {}")]
        [TestCase("func SomeFunc(bool x, double y, int z, string ranOutOfAlphabet) -> void {}")]
        [TestCase("func SomeFunc(bool x, double y, int z, string ranOutOfAlphabet) -> bool {}")]
        [TestCase("func SomeFunc(bool x, double y, int z, string ranOutOfAlphabet) -> ImagineThisIsACustomDefinedType {}")]
        public void Parse_Function_Definitions_No_Errors_No_Body(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, _) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.IsEmpty(errors);
        }

        [TestCase("export func SomeFunc(bool x) -> void {var x = 5}")]
        [TestCase("func SomeFunc(bool x, double y, int z) -> int \n {\n var x = 5;\n auto y = 6;\n return x + y; \n }\n")]
        public void Parse_Function_Definitions_Variable_Declaration_In_Body_No_Errors(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, _) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.IsEmpty(errors);
        }

        [TestCase("export func SomeFunc() -> double {", 1)]
        public void Parse_Function_Definition_N_Errors(string code, int amountOfErrors)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, _) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);
            Assert.AreEqual(amountOfErrors, errors.Length);
        }

        [TestCase("SomeFunc();")]
        [TestCase("SomeFunc(param1);")]
        [TestCase("SomeFunc(param1, param2);")]
        [TestCase("SomeFunc(param1,param2,param3);")]
        [TestCase("SomeFunc(param1, param2, param3);")]
        [TestCase("SomeFunc(param1, param2, param3)")]
        [TestCase("SomeFunc(param1,param2,param3)")]
        [TestCase("SomeFunc(param1, param2)")]
        [TestCase("SomeFunc(param1)")]
        [TestCase("SomeFunc()")]
        [TestCase("SomeFunc(callFunc());")]
        [TestCase("SomeFunc(callFunc(),callFunc2());")]
        [TestCase("SomeFunc(callFunc(), callFunc2());")]
        [TestCase("SomeFunc(callFunc(), callFunc2())")]
        [TestCase("SomeFunc(callFunc(),callFunc2())")]
        [TestCase("SomeFunc(callFunc())")]
        public void Parse_Function_Calls_No_Errors(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, _) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.IsEmpty(errors);
        }


        [TestCase("import \"this\\is\\a\\path\\\";")]
        public void Parse_Import_Statement_No_Syntax_Errors(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, _) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.IsEmpty(errors.Where(e => !e.Contains("could not be found")));
        }
        
        [TestCase("import ;")]
        [TestCase("import \"\";")]
        [TestCase("import \"fileDoesNotExist\";")]
        public void Parse_Import_Statement_Errors(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, _) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.IsNotEmpty(errors);
        }
    }
}
