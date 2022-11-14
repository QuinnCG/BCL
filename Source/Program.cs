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
        string path;

        while (true)
        {
            Console.WriteLine("Enter file to compile.");
            var input = Console.ReadLine();
            path = Directory.GetCurrentDirectory() + "\\Programs\\" + input;

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (File.Exists(path))
            {
                // Make sure the file extension is ".bcl".
                if (input.Split('.')[1] is "bcl")
                {
                    filename = input;
                    break;
                }
                else
                {
                    Console.WriteLine("File must end with \".bcl\".");
                }
            }

            Console.Clear();
			Console.WriteLine($"Could not find file {input} in \"Program\" folder.");
		}

        string source = File.ReadAllText(path);
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
		ProcessStartInfo info = new(@"Externals\ilasm.exe", filepath)
		{
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		using var ilProcess = Process.Start(info);
        if (ilProcess is null)
        {
            throw new Exception("Unknown error.");
        }

        ilProcess.WaitForExit();

		Console.WriteLine();
		if (!string.IsNullOrWhiteSpace(ilProcess.StandardError.ReadToEnd()))
        {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Ilasm.exe reported an error while trying to compile the intermediate \".il\" file.");
            Console.WriteLine("This is likely because of an unreported error in the BCL source code of the compiled program.");
        }
        else
        {
            Console.WriteLine("The exe file was compiled succesfully.");
            Console.WriteLine();

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine("The .il file that was generated with it is just an intermediate file.");
            Console.WriteLine("You can safely delete it as the program has no need for it anymore.");
            Console.WriteLine("It's been left there if you're curios what IL code was generated for the program.");
        }

        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Press any key to close...");
        Console.ReadKey(true);
    }
}
