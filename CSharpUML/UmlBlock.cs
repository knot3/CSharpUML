using System;
using System.Collections.Generic;

namespace CSharpUML
{
	public class UmlBlock : Block<UmlBlock>
	{
		public string[] comments;

		public UmlBlock (string name, UmlBlock[] content, string[] comments)
			: base(name, content)
		{
			this.comments = comments;
		}

		public UmlBlock (string name, string[] comments)
			: base(name)
		{
			this.comments = comments;
		}
	}
}

