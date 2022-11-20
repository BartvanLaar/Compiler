﻿using Exceptions;
using Lexing;
using Parsing.AbstractSyntaxTree.Expressions;
using TestHelpers.Tests;

namespace Parsing.Tests
{
    public class ParserTests
    {
        [TestCase("x + 5;", 1)]
        [TestCase("x+5;", 1)]
        [TestCase("x+5;x+=5;", 2)]
        [TestCase("x+5;x+=5;", 2)]
        [TestCase("x+5*5/5*5/5;", 1)]
        [TestCase("var x = 10*5;", 1)]
        [TestCase("var x = 10*(10-5);", 1)]
        [TestCase("var x = 10*(10-(5-2));", 1)]
        [TestCase("var x = (10*(10-(5-2)));", 1)]
        [TestCase("(10-1)*5;", 1)]
        [TestCase("var x = -5;", 1)]
        public void General_Code_Test_Throw_No_Error(string code, int expectedAmountOfTrees, int expectedAmountOfErrors = 0)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);
            var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.That(ast.First().ExpressionTree.Count(), Is.EqualTo(expectedAmountOfTrees));
            Assert.That(errors.Length, Is.EqualTo(expectedAmountOfErrors));
        }

        [TestCase("if(true){}", 1)]
        [TestCase("if(true){}else{}", 1)]
        [TestCase("if(true){}else if(false){}", 1)]
        [TestCase("if(true){}else if(false){} else {}", 1)]
        [TestCase("if(true){}else if(false){}else if(false){}else if(false){}else if(false){} else {}", 1)]
        [TestCase("var x = 0d;if(true){}", 2)]
        [TestCase("func main() -> int {if(true){return 10;} else {return 50;}}", 1)]

        public void General_Code_Test_Throw_No_Error_If_Statements(string code, int expectedAmountOfExpressionTrees, int expectedAmountOfErrors = 0)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);
            Assert.That(ast.First().ExpressionTree.Count(), Is.EqualTo(expectedAmountOfExpressionTrees));
            Assert.That(errors.Length, Is.EqualTo(expectedAmountOfErrors));
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
            Assert.That(ast.First().ExpressionTree.Count(), Is.EqualTo(expectedAmountOfExpressionTrees));
            Assert.That(errors.Length, Is.EqualTo(expectedAmountOfErrors));
        }

        [TestCase("for(var x = 0; x < 100; x = x + 1) {}", 1)]
        [TestCase("for(var x = 0; x < 100; x = x + 1) {var y = 5;}", 1)]
        public void General_Code_Test_Throw_No_Error_For_Loop_Statements(string code, int expectedAmountOfExpressionTrees, int expectedAmountOfErrors = 0)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);
            Assert.That(ast.First().ExpressionTree.Count(), Is.EqualTo(expectedAmountOfExpressionTrees));
            Assert.That(errors.Length, Is.EqualTo(expectedAmountOfErrors));
        }

        [TestCase("x--;")]
        [TestCase("x++;")]
        [TestCase("--x;")]
        [TestCase("++x;")]
        public void General_Code_Test_Throw_No_Error_MinusMinus_And_PlusPlus(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);
            Assert.That(ast.First().ExpressionTree.Count(), Is.EqualTo(1));
            Assert.IsEmpty(errors);
        }

        [TestCase("5--;")]
        [TestCase("5++;")]
        [TestCase("--5;")]
        [TestCase("++5;")]
        public void General_Code_Test_Throw_1_Error_MinusMinus_And_PlusPlus(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, ast) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.That(ast.First().ExpressionTree.Count(), Is.EqualTo(1));
            Assert.That(errors.Count(), Is.EqualTo(1));
        }


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

            Assert.That(ast.First().ExpressionTree.Count(), Is.EqualTo(1));
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

        //[TestCase("export func SomeFunc(bool x) -> void {var x = 5}")]
        [TestCase("func SomeFunc() -> int {int x = 5; return x;}")]
        [TestCase("func SomeFunc(bool x, double y, int z) -> int \n {\n var x = 5;\n auto y = 6;\n return x + y; \n }\n")]
        public void Parse_Function_Definitions_Variable_Declaration_In_Body_No_Errors(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, _) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.IsEmpty(errors);
        }

        [TestCase("export func SomeFunc() -> double {")]
        [TestCase("export func SomeFunc(var x) -> double {}")] // test fails because parsing does currently not throw an exception or abort on error. probably should..
        [TestCase("export func SomeFunc(auto x) -> double {}")] // test fails because parsing does currently not throw an exception or abort on error. probably should..
        public void Parse_Function_Definition_Throws_Error(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            Assert.Throws<SyntaxErrorException>(() => parser.Parse());
        }

        [TestCase("SomeFunc();")]
        [TestCase("SomeFunc(param1);")]
        [TestCase("SomeFunc(param1, param2);")]
        [TestCase("SomeFunc(param1,param2,param3);")]
        [TestCase("SomeFunc(param1, param2, param3);")]
        [TestCase("SomeFunc(callFunc());")]
        [TestCase("SomeFunc(callFunc(),callFunc2());")]
        [TestCase("SomeFunc(callFunc(), callFunc2());")]
        public void Parse_Function_Calls_No_Errors(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            var (errors, _) = StandardOutputHelper.RunActionAndCaptureStdOut(parser.Parse);

            Assert.IsEmpty(errors);
        }


        [TestCase("import \"this\\is\\a\\path\\\";")]
        [TestCase("import \"this\\is\\a\\path\\\" as test;")]
        public void Parse_Import_Statement_No_Syntax_Errors(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            try
            {
                parser.Parse();
            }
            catch (SyntaxErrorException ex)
            {
                if (ex.Message.ToLower().Contains("could not be found"))
                {
                    Assert.Pass();
                }
                Assert.Fail();
            }
        }

        [TestCase("import ;")]
        [TestCase("import \"\";")]
        [TestCase("import \"fileDoesNotExist\";")]
        [TestCase("import \"fileDoesNotExist\" as ;")]
        public void Parse_Import_Statement_Errors(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            Assert.Throws<SyntaxErrorException>(() => parser.Parse());
        }

        [Test]
        public void Parse_MemberAccessExpression()
        {
            var lexer = new Lexer("parent.member;");
            var parser = new Parser(lexer);
            var res = parser.Parse();
            Assert.IsNotNull(res);
            Assert.That(res.First().ExpressionTree.Count(), Is.EqualTo(1));
            Assert.That(res.First().ExpressionTree.First(), Is.TypeOf<MemberAccessExpression>());

            lexer = new Lexer("parent.member = 5;");
            parser = new Parser(lexer);
            res = parser.Parse();
            Assert.IsNotNull(res);
            Assert.That(res.First().ExpressionTree.Count(), Is.EqualTo(1));
            Assert.That(res.First().ExpressionTree.First(), Is.TypeOf<BinaryExpression>());
            Assert.That(((BinaryExpression)res.First().ExpressionTree.First()).LeftHandSide, Is.TypeOf<MemberAccessExpression>());


            lexer = new Lexer("var x = parent.member;");
            parser = new Parser(lexer);
            res = parser.Parse();
            Assert.IsNotNull(res);
            Assert.That(res.First().ExpressionTree.Count(), Is.EqualTo(1));
            Assert.That(res.First().ExpressionTree.First(), Is.TypeOf<VariableDeclarationExpression>());
            Assert.That(((VariableDeclarationExpression)res.First().ExpressionTree[0]).ValueExpression, Is.TypeOf<MemberAccessExpression>());
        }

        [Test]
        public void Parse_ContextDefinitionExpression()
        {
            var lexer = new Lexer("context this.is.the.context;");
            var parser = new Parser(lexer);
            var res = parser.Parse();
            Assert.IsNotNull(res);
            Assert.That(res.First().ExpressionTree.Count(), Is.EqualTo(1));
            Assert.That(res.First().ExpressionTree.First(), Is.TypeOf<ContextDefinitionExpression>());

        }

        [Test]
        public void Parse_ContextDefinitionExpression_Errors()
        {
            var lexer = new Lexer("context this.is.the.context");
            var parser = new Parser(lexer);
            Assert.Throws<SyntaxErrorException>(() => parser.Parse());

            lexer = new Lexer("context");
            parser = new Parser(lexer);
            Assert.Throws<SyntaxErrorException>(() => parser.Parse());

            lexer = new Lexer("this.is.the.context");
            parser = new Parser(lexer);
            Assert.Throws<SyntaxErrorException>(() => parser.Parse());
        }
    }
}
