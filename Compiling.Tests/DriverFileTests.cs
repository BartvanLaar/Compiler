using NUnit.Framework;
using System.Collections;
using System.Diagnostics;
using System.IO;
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
            public int? ExitCode { get; set; }
        }


        private class TestFiles : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return new object[] { FN("VoidMainFuncNoParamsDefinition"), new TestFile() { IsExecutable = true} }; // null exit code cause void main...
                yield return new object[] { FN("VoidLibFuncNoParamsDefinition"), new TestFile() { IsExecutable = false } };
                yield return new object[] { FN("IntMainFuncNoParamsDefinition"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IntMainFuncNoParamsDefinitionCall"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("DoubleMainFuncNoParamsDefinition"), new TestFile() { IsExecutable = true } };
                yield return new object[] { FN("DoubleMainFuncNoParamsDefinitionCall"), new TestFile() { IsExecutable = true } };
                yield return new object[] { FN("DoubleMainFuncNoParamsDefinitionReturnIntegerButTypeCheckShouldFix"), new TestFile() { IsExecutable = true} };
                yield return new object[] { FN("IntMainFuncNoParamsDefinitionReturnFunctionCall"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IntMainFuncNoParamsDefinitionReturnLocalVariableAsVar"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IntMainFuncNoParamsDefinitionReturnLocalVariableAsAuto"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IntMainFuncNoParamsDefinitionReturnLocalVariableAsInt"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IntMainFuncParamsDefinition"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IntMainFuncStoreFuncCallResIn2VarAndReturn"), new TestFile() { IsExecutable = true, ExitCode = 84 } };
                yield return new object[] { FN("IfFalseElseIfFalseElseTest"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IfFalseElseIfTrueElseTest"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IfFalseElseIfTrueTest"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IfFalseElseTest"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IfTrueElseTest"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IfTrueReturn"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IfFalseReturn"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IfTrueElseTest"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IfTrueIfFalseElseNestedElseTest"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("IfTrueIfTrueElseNestedElseTest"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                yield return new object[] { FN("ForILoopReturn"), new TestFile() { IsExecutable = true, ExitCode = 42 } };
                //test below will fail untill we have proper type checking and inference (with required added type metadata) to support function name mangling at the calling side...
                yield return new object[] { FN("IntMainFuncParamsDefinitionOverloaded"), new TestFile() { IsExecutable = true, ExitCode = 42 } };

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
            Driver.RunLLVM(file, filename, testFile.IsExecutable, useClangCompiler: false);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.ChangeExtension(filename, testFile.Extension));
            if (File.Exists(filePath))
            {
                if (testFile.IsExecutable && testFile.ExitCode.HasValue)
                {
                    var proc = Process.Start(filePath);
                    proc.WaitForExit();
                    Assert.AreEqual(proc.ExitCode, testFile.ExitCode);
                }
                TryRemoveResultsOfTest(filePath);
                Assert.Pass();
            }
            //probably want to keep the files of failed tests for diagnose purposes..
            //TryRemoveResultsOfTest(filePath);
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
