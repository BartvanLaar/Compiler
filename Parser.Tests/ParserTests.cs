using NUnit.Framework;
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Expressions;
using Parser.CodeLexer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Tests
{
    internal class ParserTests
    {
        private class DummyListener : IParserListener
        {
            public void EnterHandleAssignmentExpression(AssignmentExpression data)
            {
            }

            public void EnterHandleTopLevelExpression(FunctionCallExpression data)
            {
            }

            public void ExitHandleAssignmentExpression(AssignmentExpression data)
            {
            }

            public void ExitHandleTopLevelExpression(FunctionCallExpression data)
            {
            }
        }

        [TestCase("x + 5;")]
        [TestCase("x + y;")]
        [TestCase("x - 5;")]
        [TestCase("x - y;")]
        [TestCase("x * 5;")]
        [TestCase("x * y;")]
        [TestCase("x / 5;")]
        [TestCase("x / y;")]
        public void Test_valid_parsings(string code)
        {
            var lexer = new Lexer(code);
            var parser = new Parser(lexer, new DummyListener());

            using var sw = new StringWriter();
            Console.SetOut(sw);

            parser.Parse();

            Assert.IsEmpty(sw.ToString());
        }
    }
}
