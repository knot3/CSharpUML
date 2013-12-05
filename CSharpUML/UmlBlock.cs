using System;

namespace CSharpUML
{
	public class UmlBlock : Block<UmlBlock>
	{
		public UmlBlock (string name, UmlBlock[] content)
			: base(name, content)
		{
		}

		public UmlBlock (string name)
			: base(name)
		{
		}
	}
}

