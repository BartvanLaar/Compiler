using Compiling;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Compiling.Tests
{
    internal class DriverTests
    {
        // The tests below don't do ... atm. Might as well be Assert.Pass();...
        [TestCase("2 + 2;")]
        [TestCase("2 + 2d;")]
        [TestCase("2 + 2f;")]
        [TestCase("2 + 2.0;")]
        [TestCase("2 + 3f + 5d;")]
        [TestCase("2 - 2;")]
        [TestCase("2 / 2;")]
        [TestCase("2 * 2;")]
        [TestCase("x = 2;")]
        [TestCase("var x = 2;")]
        [TestCase("var x = 2 + 3;")]
        [TestCase("var x = 2 / 3;")]
        [TestCase("var x = 2 * 3;")]
        [TestCase("var x = 2 - 3;")]

        [TestCase("var x = 2 - 3; var x = 2 - 3;")]
        [TestCase("var x = 2 - 3; \n var x = 2 - 3;")]
        [TestCase("var x = 2 - 3; \r var x = 2 - 3;")]
        [TestCase("var x = 2 - 3; \r\n var x = 2 - 3;")]
        [TestCase("var x = 2 - 3; x = 2 - 3;")]
        [TestCase("var x = 2 - 3; \n x = 2 - 3;")]
        [TestCase("var x = 2 - 3; \r x = 2 - 3;")]
        [TestCase("var x = 2 - 3; \r\n x = 2 - 3;")]

        [TestCase("auto x = 2;")]
        [TestCase("auto x = 2 + 3;")]
        [TestCase("auto x = 2 / 3;")]
        [TestCase("auto x = 2 * 3;")]
        [TestCase("auto x = 2 - 3; auto x = 2 - 3;")]
        [TestCase("auto x = 2 - 3; \n auto x = 2 - 3;")]
        [TestCase("auto x = 2 - 3; \r auto x = 2 - 3;")]
        [TestCase("auto x = 2 - 3; \r\n auto x = 2 - 3;")]
        [TestCase("auto x = 2 - 3; x = 2 - 3;")]
        [TestCase("auto x = 2 - 3; \n x = 2 - 3;")]
        [TestCase("auto x = 2 - 3; \r x = 2 - 3;")]
        [TestCase("auto x = 2 - 3; \r\n x = 2 - 3;")]
        public void Test(string code)
        {
            Driver.Run(code);
        }

        [Test]
        public void Driver_Test_LLVM_1()
        {
            Driver.RunLLVM("(10-1)*5;");
        }

        [TestCase("(10-1)*5;", 45)]
        [TestCase("10-1*5;",5)]
        [TestCase("10-(1*5);",5)]
        [TestCase("10*(5-1);",40)]
        [TestCase("10*5-1;",49)]
        [TestCase("10-5-1;",4)]
        [TestCase("(10*(5-1));",40)]
        [TestCase("(10-2)*(5-1);",32)]
        [TestCase("((10-2)*(5-1));",32)]
        [TestCase("((10-(2+2))-(5-1));",2)]
        [TestCase("((10-(2+2))*(5-1));",24)]
        [TestCase("(10-(2+2))-5;",1)]
        public void Driver_Test_Log_1(string code,double expectedResult)
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            Driver.RunDotNet(code);
            var messages = sw.ToString().Split("\n").Where(s => !string.IsNullOrWhiteSpace(s));
            Assert.AreEqual(expectedResult,double.Parse(messages.Last()));
        }
    }
}
