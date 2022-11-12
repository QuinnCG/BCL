using System.Text;

namespace BCL;

// This class generates the CIL code based on a list of "Expression" objects.
internal class Generator
{
	// A string builder that's used to build the final CIL code file.
	public StringBuilder Builder = new();

	public Generator(string name, List<Expression> expressions)
	{
		// This imports a dll.
		// "mscorlib" holds a lot of .NET's utilities.
		Builder.AppendLine(".assembly extern mscorlib {}");

		// This declares an assembly for the program being made.
		Builder.AppendLine();
		Builder.AppendLine($".assembly {name}");
		Builder.AppendLine("{");
		// The version of the assembly.
		Builder.AppendLine("\t.ver 1:0:0:0");
		Builder.AppendLine("}");

		// Assemblies in .NET must have at least one module.
		// In this case it's always an executable module.
		// The other common type of module is a dll module.
		Builder.AppendLine();
		Builder.AppendLine($".module {name}.exe");

		// This is the main method for the prorgam.
		// "cil managed" means this method is managed code and not from an external unamanged source like a C binary.
		Builder.AppendLine();
		Builder.AppendLine(".method static void main() cil managed");
		Builder.AppendLine("{");
		// This marks this method as the entrypoint method.
		Builder.AppendLine("\t.entrypoint");
		// This is the maximum number of variables that can be on the "stack".
		// CIL is a "stack-based" assembly language.
		// Stack-based languages load one or more variables onto the stack then operate on them before "popping" them off the stack.
		Builder.AppendLine("\t.maxstack 2");

		// From the BCL source code obtains every expressions who's command name is "var".
		// This is needed because some BCL code will act differently depending on the type of a variable.
		Dictionary<string, Expression> varExpressions = new();
		foreach (var expression in expressions)
		{
			if (expression.Command is "var")
			{
				varExpressions.Add(expression.Arguments[1], expression);
			}
		}

		// The format is:
		// .locals init (int32 myInt, string myStr)
		// In CIL any variables used in the scope of their enclosing method are declared here.
		List<string> locals = new();
		int i = 0;
		foreach (var ve in varExpressions)
		{
			string end = i == varExpressions.Count - 1 ? "" : ", ";
			string str = $"{ConvertToILType(ve.Value.Arguments[0])} {ve.Value.Arguments[1]}{end}";
			locals.Add(str);

			i++;
		}

		Builder.AppendLine();
		Builder.AppendLine($"\t.locals init ({string.Concat(locals)})");

		// For each expression in the expressions list add one or more lines of CIL code to the class's StringBuilder "Builder".
		foreach (var expression in expressions)
		{
			// For declaring variables.
			if (expression.Command is "var")
			{
				if (expression.Arguments.Length < 3)
				{
					continue;
				}

				Builder.AppendLine();

				// Depending on the BCL type use the corrosponding CIL load-constant instruction.
				// This will load a value onto the stack.
				string loadType = expression.Arguments[0] switch
				{
					"int" => "ldc.i4",
					"long" => "ldc.i8",
					"float" => "ldc.r4",
					"double" => "ldc.r8",
					"string" => "ldstr",

					// Crash the compiler if an invalid type is given.
					_ => throw new NotSupportedException($"BCL type \"{expression.Arguments[0]}\" is not a valid type.")
				};

				// In BCL string variables don't have quotes.
				// Instead every command argument after the variable name is considered part of the string.
				if (expression.Arguments[0] is "string")
				{
					string[] stringParts = expression.Arguments[2..];
					string str = string.Concat(stringParts.Select((str, i) =>
					{
						if (i == stringParts.Length - 1)
							return str;
						else
							return str + " ";
					}));

					// Load string variable onto stack.
					Builder.AppendLine($"\t{loadType} \"{str}\"");
				}
				// If the type is not a string then there's less to do.
				else
				{
					// Load variable onto stack.
					Builder.AppendLine($"\t{loadType} {string.Concat(expression.Arguments[2..])}");
				}

				// Store the topmost stack variable in a local variable.
				Builder.AppendLine($"\tstloc {expression.Arguments[1]}");
			}
			// For calling any build-in functions.
			else if (expression.Command is "call")
			{
				Builder.AppendLine();

				// The first argument of the expression is the name of the function to call.
				string callType = expression.Arguments[0];
				// Every argument after that is an argument for the function.
				string[] callArgs = expression.Arguments[1..];

				// Prints a variable to the console.
				if (callType is "print")
				{
					// The name of the variable to print.
					string specifier = callArgs[0];
					// Load the variable onto the stack.
					Builder.AppendLine($"\tldloc {specifier}");

					// Call the corrosponding .NET Console.WriteLine function.
					// There are multiple versions, one for each basic .NET type.
					string sourceType = varExpressions[callArgs[0]].Arguments[0];
					string ilType = ConvertToILType(sourceType);
					Builder.AppendLine($"\tcall void [mscorlib]System.Console::WriteLine({ilType})");
				}
				// Operators on two variables and stores the result a variable.
				else if (callType is "add" or "subtract" or "multiply" or "divide")
				{
					// Gets the corrosponding CIL instruction.
					string operation = callType switch
					{
						"add" => "add",
						"subtract" => "sub",
						"multiply" => "mul",
						"divide" => "div",

						// This would never be called.
						// Visual Studio gives a warning if you don't implement a default-case so this is here for that.
						_ => throw new NotSupportedException()
					};

					// Gets the name of the input varaible
					string inputA = callArgs[0];
					string inputB = callArgs[1];
					string output = callArgs[2];

					Builder.AppendLine($"\tldloc {inputA}");
					Builder.AppendLine($"\tldloc {inputB}");
					Builder.AppendLine($"\t{operation}");

					Builder.AppendLine($"\tstloc {output}");
				}
				else if (callType is "goto")
				{
					if (callArgs.Length > 1)
					{
						string branchType = callArgs[2] switch
						{
							"=" => "beq",
							"<" => "blt",
							">" => "bgt",
							">=" => "bge",
							"<=" => "ble",
							"!" => "bne.un",

							_ => throw new NotSupportedException()
						};

						Builder.AppendLine($"\tldloc {callArgs[1]}");
						Builder.AppendLine($"\tldloc {callArgs[3]}");
						Builder.AppendLine($"\t{branchType} _{callArgs[0]}");
					}
					else
					{
						Builder.AppendLine($"\tbr _{callArgs[0]}");
					}
				}
				else if (callType is "read")
				{
					Builder.AppendLine("\tcall string [mscorlib]System.Console::ReadLine()");

					if (varExpressions[callArgs[0]].Arguments[0] is not "string")
					{
						string type = varExpressions[callArgs[0]].Arguments[0];
						type = ConvertToILType(type);
						Builder.AppendLine($"\tcall {type} [mscorlib]System.{char.ToUpper(type[0]) + type[1..]}::Parse(string)");
					}

					Builder.AppendLine($"\tstloc {callArgs[0]}");
				}
				else if (callType is "pause")
				{
					Builder.AppendLine("\tcall string [mscorlib]System.Console::ReadLine()");
					Builder.AppendLine("\tpop");
				}
			}
			// Used for the goto function.
			// The goto function will make the program execute from the given label.
			else if (expression.Command is "label")
			{
				// In CIL labels are declared with a name followed by a semi-colon.
				// To avoid name conflicts such as "true" not being allowed in CIL, the generated code prefixes labels with an underscore.
				Builder.AppendLine();
				Builder.AppendLine($"\t_{expression.Arguments[0]}:");
			}
			else
			{
				// Crash the compiler if an invalid expression is somehow inputted.
				throw new NotSupportedException($"Invalid expression {expression}");
			}
		}

		// In CIL, methods must call ret (return) even if they return no value.
		Builder.AppendLine();
		Builder.AppendLine("\tret");
		Builder.AppendLine("}");
	}

	// This is a utility method for converting BCL language types into their CIL equivalent.
	private static string ConvertToILType(string sourceType) => sourceType switch
	{
		// A standard 4-byte integer. Represents a whole number.
		"int" => "int32",
		// An 8-byte integer. Represents a whole number.
		"long" => "int64",
		// A standard 4-byte floating-point number. Can have a decimal.
		"float" => "float32",
		// An 8-byte floating-point number. Can have a decimal.
		"double" => "float64",
		// A string object. Represents text.
		"string" => "string",

		// Crash the compiler if an invalid type is somehow inputted.
		_ => throw new NotImplementedException()
	};
}
