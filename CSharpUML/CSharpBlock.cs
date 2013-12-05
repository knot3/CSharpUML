using System;
using System.Linq;
using System.Collections.Generic;

namespace CSharpUML
{
	public sealed class CSharpBlock : Block<CSharpBlock>
	{
		public CSharpBlock (string name, CSharpBlock[] content)
			: base(name, content)
		{
		}

		public CSharpBlock (string name)
			: base(name)
		{
		}
	}
}

