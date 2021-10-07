using Lexing;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

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
        [TestCase("var x = 10*(10-5);", 1)] // Known to fail () are not yet supported when doing math stuff.
        public void Test(string code, int expectedAmountOfTrees, int expectedAmountOfErrors = 0)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            using var sw = new StringWriter();
            Console.SetOut(sw);

            var ast = parser.Parse();
            Assert.AreEqual(expectedAmountOfTrees, ast.Count);

            var errors = sw.ToString().Split("\n").Where(s => !string.IsNullOrWhiteSpace(s));
            Assert.AreEqual(expectedAmountOfErrors, errors.Count());
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
        public void Test_Valid_Parsings_No_Errors(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer);

            using var sw = new StringWriter();
            Console.SetOut(sw);

            var ast = parser.Parse();
            Assert.AreEqual(1, ast.Count);
            Assert.IsEmpty(sw.ToString());
        }
    }
}
