using System;
using System.Collections.Generic;

namespace CSharpUML
{
	public interface IParser
	{
		IEnumerable<IUmlObject> Parse (IEnumerable<string> lines);

		IEnumerable<IUmlObject> Parse (string filename);
	}
}

