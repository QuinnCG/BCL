using System.Text;

namespace BCL;

// This represnts a line of BCL code.
internal struct Expression
{
	public string Command;
	public string[] Arguments;

	public Expression(string cmd, string[] args)
	{
		Command = cmd;
		Arguments = args;
	}

	// For debugging purposes its handy to have an easy way to print to console the value of an expression.
	public override string ToString()
	{
		StringBuilder builder = new();

		// ", " is added to the end of each argument, unless its the last argument.
		int i = 0;
		foreach (var arg in Arguments)
		{
			string end = i == Arguments.Length - 1 ? "" : ", ";
			builder.Append(arg + end);

			i++;
		}

		return $"{Command}, {builder}";
	}
}
