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
            public string Extension => IsExecutable ? ".exe" : ".dll";
        }

        private class TestFiles : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return new object[] { FN("VoidMainFuncNoParamsDefinition"), new TestFile() { IsExecutable = true } };
                yield return new object[] { FN("VoidLibFuncNoParamsDefinition"), new TestFile() { IsExecutable = false } };
                yield return new object[] { FN("IntMainFuncNoParamsDefinition"), new TestFile() { IsExecutable = true } };
                yield return new object[] { FN("IntMainFuncNoParamsDefinitionCall"), new TestFile() { IsExecutable = true } };
                yield return new object[] { FN("DoubleMainFuncNoParamsDefinition"), new TestFile() { IsExecutable = true } };
                yield return new object[] { FN("DoubleMainFuncNoParamsDefinitionCall"), new TestFile() { IsExecutable = true } };
                yield return new object[] { FN("DoubleMainFuncNoParamsDefinitionReturnFunctionCall"), new TestFile() { IsExecutable = true } };

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
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.ChangeExtension(filename, testFile.Extension));
            if (File.Exists(filePath))
            {
                TryRemoveResultsOfTest(filePath);
                Assert.Pass();
            }
            TryRemoveResultsOfTest(filePath);
            Assert.Fail();
        }

        private static void TryRemoveResultsOfTest(string filePath)
        {
            try { File.Delete(filePath); } catch { }
            try { File.Delete(Path.ChangeExtension(filePath, ".obj")); } catch { }
            try { File.Delete(Path.ChangeExtension(filePath, ".lib")); } catch { }
            try { File.Delete(Path.ChangeExtension(filePath, ".bc")); } catch { }
            try { File.Delete(Path.ChangeExtension(filePath, ".s")); } catch { }
            try { File.Delete(Path.ChangeExtension(filePath, ".o")); } catch { }
        }
    }
}
