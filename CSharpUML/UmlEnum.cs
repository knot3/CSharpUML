using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUML
{
	public class UmlEnum : UmlObject
	{
		private string[] values;

		public UmlEnum (CSharpBlock block)
			: base(block)
		{
			name = name.IfContains ("enum ", () => {});
			values = ParseContent (block.Content).ToArray ();
		}

		public UmlEnum (UmlBlock block)
			: base(block)
		{
			name = name.IfContains ("enum ", () => {});
			values = ParseContent (block.Content).ToArray ();
		}

		public IEnumerable<string> ParseContent (IEnumerable<CSharpBlock> blocks)
		{
			foreach (CSharpBlock subblock in blocks) {
				foreach (string value in subblock.Name.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).TrimAll()) {
					yield return value;
				}
			}
		}

		public IEnumerable<string> ParseContent (IEnumerable<UmlBlock> blocks)
		{
			foreach (UmlBlock subblock in blocks) {
				yield return subblock.Name.TrimAll ();
			}
		}

		public static bool Matches (CSharpBlock block)
		{
			return block.Name.Contains ("enum");
		}

		public static bool Matches (UmlBlock block)
		{
			return block.Name.Contains ("enum");
		}

		public override string ToUmlCode (int padding = 0)
		{
			string paddingStr = String.Concat (Enumerable.Repeat (" ", padding));
			List<string> lines = new List<string> ();
			lines.Add (paddingStr + Publicity.ToCode ("", " ") + Virtuality.ToCode ("", " ") + "enum " + name);
			foreach (string value in values) {
				lines.Add (paddingStr + "    " + value);
			}
			return string.Join ("\n", lines);
		}
	}

}

