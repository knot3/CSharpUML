using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUML
{
	public class CSharpParser : IParser
	{
		public CSharpParser ()
		{
		}

		public CSharpBlock[] Parse (char[] chars, ref int i)
		{
			List<CSharpBlock> blocks = new List<CSharpBlock> ();
			string desc = "";
			int inBrackets = 0;
			for (; i < chars.Length; ++i) {
				char c = chars [i];
				if (c == '(')
					++inBrackets;
				else if (c == ')')
					--inBrackets;
				if (c == '{' && inBrackets == 0) {
					desc = desc.Split (new string[]{ ": base" }, StringSplitOptions.None) [0];
					desc = desc.TrimAll ();
					//if (desc.Count () > 0) {
					++i;
					CSharpBlock block = new CSharpBlock (
							name: desc,
							content: Parse (chars, ref i)
					);
					blocks.Add (block);
					desc = "";
				} else if (c == '}' && inBrackets == 0) {
					desc = desc.TrimAll ();
					break;
				} else if (c == ';' && inBrackets == 0) {
					desc = desc.Split (new string[]{ ": base" }, StringSplitOptions.None) [0];
					desc = desc.TrimAll ();
					if (desc.Count () > 0) {
						CSharpBlock block = new CSharpBlock (name: desc);
						blocks.Add (block);
						desc = "";
					}
				} else {
					desc += c;
				}
			}
			desc = desc.TrimAll ();
			if (desc.Count () > 0) {
				CSharpBlock block = new CSharpBlock (name: desc);
				blocks.Add (block);
			}
			return blocks.ToArray ();
		}

		public IEnumerable<IUmlObject> Parse (IEnumerable<string> lines)
		{
			char[] chars = StripComments (lines).Where (FilterEmptyLines).ToCharacters ().ToArray ();

			int i = 0;
			CSharpBlock[] blocks = Parse (chars, ref i);
			foreach (CSharpBlock np in blocks) {
				if (np.Name.Contains ("namespace")) {
					foreach (CSharpBlock block in np.Content) {
						if (UmlClass.Matches (block)) {
							yield return new UmlClass (block);
						} else if (UmlEnum.Matches (block)) {
							yield return new UmlEnum (block);
						}
					}
				}
			}
		}

		private IEnumerable<string> StripComments (IEnumerable<string> lines)
		{
			foreach (string _line in lines) {
				string line = _line.Split (new string[]{"//", "#"}, StringSplitOptions.None) [0].Trim ();
				if (line.Length > 0) {
					yield return line;
				}
			}
		}

		private bool FilterEmptyLines (string line)
		{
			return line.Trim ().Length > 0;
		}

		public IEnumerable<IUmlObject> Parse (string filename)
		{
			IEnumerable<IUmlObject> parsed = Parse (Files.ReadLines (filename).TrimAll ());
			foreach (IUmlObject obj in parsed) {
				yield return obj;
			}
		}
	}
}

