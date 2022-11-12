// BCL stands for "Basic Command Language".
// Its similar in design to writing console commands in Command Prompt.
// Each line is a command. Parts of a command are seperated by spaces. The first part is the command's name and the rest are it's arguments.
// BCL compiles to CIL; Microsoft's "Common Intermediate Language".
// CIL is a stack-based, object-oriented, assembly language that languages such as C# compile into to.

using System.Diagnostics;

namespace BCL;

internal static class Program
{
    static void Main()
    {
        string filename;
        while (true)
        {
            Console.WriteLine("Enter file to compile.");
            var input = Console.ReadLine();

            if (File.Exists(input))
            {
                // Make sure the file extension is ".bcl".
                if (input.Split('.')[1] is "bcl")
                {
                    filename = input;
                    break;
                }
            }

            Console.Clear();
        }

        string source = File.ReadAllText(filename);
        Parser parser;
        Generator generator;

        try
        {
            parser = new Parser(source);
            generator = new Generator(filename.Split('.')[0], parser.Expressions);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[Compiler Error]: ");
            Console.WriteLine(e.Message);
            Console.WriteLine("Press any key to close...");
            Console.ReadKey(true);
            return;
        }

        string filepath = filename.Split('.')[0] + ".il";
        using var file = File.CreateText(filepath);
        file.Write(generator.Builder.ToString());
        file.Close();

        // Run "ilasm.exe" (the CIL compiler) on the generated ".il" file.
        using var ilProcess = Process.Start(@"Externals\ilasm.exe", filepath);
        ilProcess.WaitForExit();

        Console.WriteLine("Press any key to close...");
        Console.ReadKey(true);
    }
}
