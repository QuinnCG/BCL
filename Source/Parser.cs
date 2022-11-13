namespace BCL;

// This class breaks a bunch of source text down into "Expression" objects.
internal class Parser
{
	// A list of all the expressions the parser obtains.
	// An expression here means a line of code.
	public List<Expression> Expressions { get; set; } = new();

	public Parser(string source)
	{
		// Seperate the source code by the newline character.
		string[] lines = source.Split(Environment.NewLine);

		int lineNum = 0;
		foreach (var line in lines)
		{
			// Seperate the line by its spaces.
			string[] parts = line.Split(' ');

			// The first part of the line is the command name.
			string cmd = parts[0];
			// Every part after the name is the command arguments.
			string[] args = parts[1..];

			// Add expressions who's command name is "var", "call", or "label".
			if (cmd is "var" or "call" or "label")
			{
				Expressions.Add(new Expression(cmd, args));
			}
			// "//" command lines and whitespace of any sort is NOT an error.
			else if (cmd.StartsWith("//") || string.IsNullOrWhiteSpace(cmd))
			{
				continue;
			}
			else
			{
				throw new Exception($"Unknown command \"{cmd}\".");
			}

			lineNum++;
		}
	}
}
