using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUML
{
	public class UmlParser : IParser
	{
		public UmlParser ()
		{
		}

		private UmlBlock[] ParseBlocks (string[] lines, ref int i, int parentIndent)
		{
			int baseIndent = lines [i].Indentation ();
			List<UmlBlock> blocks = new List<UmlBlock> ();
			for (; i < lines.Length; ++i) {
				string line = lines [i];
				int indent = line.Indentation ();
				//Console.WriteLine(indent + "  " + line);
				if (indent < baseIndent) {
					--i;
					break;
				} else if (indent == baseIndent) {
					UmlBlock block;
					if (i + 1 < lines.Length && lines [i + 1].Indentation () > indent) {
						i += 1;
						block = new UmlBlock (
								name: line.TrimAll (),
								content: ParseBlocks (lines, ref i, indent)
						);
					} else {
						block = new UmlBlock (
								name: line.TrimAll ()
						);
					}
					blocks.Add (block);
				} else {
					throw new InvalidOperationException ("This should never happen!");
				}
			}
			return blocks.ToArray ();
		}
		
		public IEnumerable<IUmlObject> Parse (IEnumerable<string> lines)
		{
			if (lines.Count () > 0) {
				int i = 0;
				UmlBlock[] blocks = ParseBlocks (lines.ToArray (), ref i, -1);

				foreach (UmlBlock block in blocks) {
					if (UmlClass.Matches (block)) {
						yield return new UmlClass (block);
					} else if (UmlEnum.Matches (block)) {
						yield return new UmlEnum (block);
					}
				}
			}
		}

		public IEnumerable<IUmlObject> Parse (string filename)
		{
			IEnumerable<IUmlObject> parsed = Parse (Files.ReadLines (filename));
			foreach (IUmlObject obj in parsed) {
				yield return obj;
			}
		}
	}
}

