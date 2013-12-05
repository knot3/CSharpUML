using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUML
{
	public abstract class Block<T>
	{
		public string Name { get; private set; }

		public T[] Content { get; private set; }

		public Block (string name, T[] content)
		{
			Name = name;
			Content = content;
		}

		public Block (string name)
		{
			Name = name;
			Content = new T[]{};
		}

		public int Count { get { return Content.Length; } }

		public override string ToString ()
		{
			return ToString (0);
		}

		public string ToString (int padding)
		{
			string paddingStr = String.Concat (Enumerable.Repeat (" ", padding));
			List<string> lines = new List<string>();
			lines.Add(paddingStr + Name);
			foreach (T block in Content) {
				lines.Add((block as Block<T>).ToString (padding + 4));
			}
			return string.Join("\n", lines);
		}
	}
}

