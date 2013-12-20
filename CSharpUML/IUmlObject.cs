CSharpUML/CSharpUML-Linux.userprefsusing System;
using System.Collections.Generic;

using NDesk.Options;
using System.Collections;

namespace CSharpUML
{
	public interface IUmlObject : IEquatable<IUmlObject>, IComparable<IUmlObject>
	{
		string ToUmlCode(int padding = 0);
		string ToCSharpCode(int padding = 0);
		string ToTexCode();
		
		Publicity Publicity { get; }
		Virtuality Virtuality { get; }
		string Name { get; }
	}
}

