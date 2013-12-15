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

		private int nextIndent (string[] lines, int i)
		{
			int indent = -1;
			for (int j = i; j < lines.Length; ++j) {
				indent = lines [j].Indentation ();
				// Console.WriteLine ("indent = " + indent + ": " + lines [j]);
				if (!lines [j].Contains ("//"))
					break;
			}
			return indent;
		}

		private UmlBlock[] ParseBlocks (string[] lines, ref int i, int parentIndent)
		{
			int baseIndent = nextIndent (lines, i);

			List<UmlBlock> blocks = new List<UmlBlock> ();
			for (; i < lines.Length; ++i) {
				string line = lines [i];
				int indent = line.Indentation ();
				// Console.WriteLine (indent + "  " + line + "  (" + baseIndent + ")");

				if (line.Trim ().Contains ("//")) {
					//Console.WriteLine ("CurrentComments.add(" + line + ")");
					Comments.AddParsedComment (line.TrimAll ().Substring (2).TrimAll ());
					
				} else if (indent < baseIndent) {
					--i;
					break;

				} else if (indent == baseIndent) {
					UmlBlock block;
					string[] comments = Comments.CurrentComments ();
					if (i + 1 < lines.Length && nextIndent (lines, i + 1) > indent) {
						i += 1;
						block = new UmlBlock (
							name: line.TrimAll (),
							content: ParseBlocks (lines, ref i, indent),
							comments : comments
						);
					} else {
						block = new UmlBlock (
							name: line.TrimAll (),
							comments: comments
						);
					}
					blocks.Add (block);

				} else if (line.Trim ().Length > 0) {
					throw new InvalidOperationException ("This should never happen! " + line);
				}
			}
			return blocks.ToArray ();
		}
		
		public IEnumerable<IUmlObject> Parse (IEnumerable<string> lines)
		{
			if (lines.Count () > 0) {
				int i = 0;
				UmlBlock[] blocks = ParseBlocks (lines.Where (FilterIgnoreLines).ToArray (), ref i, -1);

				foreach (UmlBlock block in blocks) {
					if (UmlClass.Matches (block)) {
						yield return new UmlClass (block);
					} else if (UmlEnum.Matches (block)) {
						yield return new UmlEnum (block);
					}
				}
			}
		}

		private bool FilterIgnoreLines (string line)
		{
			return !(line.ToLower ().Contains ("attributes:") || line.ToLower ().Contains ("methods:"));
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

