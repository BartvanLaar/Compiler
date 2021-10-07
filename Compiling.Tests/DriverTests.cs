using Compiling;
using NUnit.Framework;

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
            Driver.RunLLVM("4.0 + 6.0;");
        }
    }
}
