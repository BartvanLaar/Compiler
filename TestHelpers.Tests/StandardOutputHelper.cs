namespace TestHelpers.Tests
{
    public static class StandardOutputHelper
    {

        public static string[] RunActionAndCaptureStdOut(Action action)
        {
            try
            {
                using var sw = new StringWriter();
                Console.SetOut(sw);
                action();
                var messages = sw.ToString().Split("\n").Where(s => !string.IsNullOrWhiteSpace(s));
                return messages.ToArray();
            }
            finally
            {
                var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
            }

        }

        public static (string[] stdOutput, TResult Result) RunActionAndCaptureStdOut<TResult>(Func<TResult> action)
        {
            try
            {
                using var sw = new StringWriter();
                Console.SetOut(sw);
                var res = action();
                var messages = sw.ToString().Split("\n").Where(s => !string.IsNullOrWhiteSpace(s));
                return (messages.ToArray(), res);
            }
            finally
            {
                var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
            }

        }
    }
}