using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiling.Tests
{
    internal class DriverFileTests
    {
        private const string TEST_FILE_FOLDER = "TestFiles";
        private const string TEST_FILE_EXTENSION = ".bs";
        internal class TestFile
        {
            public bool IsExecutable { get; set; }
        }

        private class TestFiles : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return new object[] { FN("VoidMainFuncNoParamsDefinition"), new TestFile() { IsExecutable = true } };
            }
        }

        private static string FN(string file)
        {
            if (!file.EndsWith(TEST_FILE_EXTENSION))
            {
                file = $"{file}{TEST_FILE_EXTENSION}";
            }

            return Path.Combine(TEST_FILE_FOLDER, file);
        }


        [TestCaseSource(typeof(TestFiles))]
        public async Task Driver_Test_Code_Files(string filepath, TestFile testFile)
        {
            var filename = Path.GetFileName(filepath);
            var file = await File.ReadAllTextAsync(filepath);
            Driver.RunLLVM(file, filename, testFile.IsExecutable);
        }
    }
}
