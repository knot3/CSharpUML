using System;
using System.Collections.Generic;

using NDesk.Options;

namespace CSharpUML
{
	public interface IUmlObject
	{
		string ToUmlCode(int padding = 0);
		
		Publicity Publicity { get; }
		Virtuality Virtuality { get; }
		string Name { get; }
	}
}

