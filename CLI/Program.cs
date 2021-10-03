using Parser;

namespace Compiler.CLI
{

    public class Program
    {

        public static void Main(params string[] args)
        {
            Console.WriteLine("Press to start.");
            Console.ReadKey();
            var code = "2 + 2;";
            Driver.RunLLVM(code);

            Console.WriteLine("Press to exit.");
            Console.ReadKey();
        }
    }
}
