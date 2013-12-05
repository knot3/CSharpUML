using System;
using System.Linq;

namespace CSharpUML
{
	public class UmlAttribute : UmlObject
	{
		public string type;

		public UmlAttribute (CSharpBlock block)
			: base(block)
		{
			name = name.Split ('=') [0].TrimAll ();

			if (name.Contains (" ")) {
				string[] p = name.Split (new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
				type = p [0];
				name = "";
				for (int i = 0; i < p.Length; ++i) {
					name += i == 0 ? "" : " " + p [i];
				}
			}
			name = name.TrimAll ();
		}

		public UmlAttribute (UmlBlock block)
			: base(block)
		{
			if (name.Contains (":")) {
				string[] p = name.Split (":").TrimAll ().ToArray ();
				name = p [0];
				type = p [1];
			}
		}

		public static bool Matches (CSharpBlock block)
		{
			string line = block.Name;
			int indexBracketOpen = line.IndexOf ("(");
			int indexBracketClose = line.IndexOf (")");
			int indexEqualSign = line.IndexOf ("=");
			if (indexBracketOpen == -1 && indexBracketClose == -1) {
				return true;
			} else if (indexEqualSign < indexBracketOpen) {
				return true;
			} else {
				return false;
			}
		}

		public static bool Matches (UmlBlock block)
		{
			string line = block.Name;
			int indexBracketOpen = line.IndexOf ("(");
			int indexBracketClose = line.IndexOf (")");
			if (indexBracketOpen == -1 && indexBracketClose == -1) {
				return true;
			} else {
				return false;
			}
		}

		public override string ToUmlCode (int padding = 0)
		{
			string paddingStr = String.Concat (Enumerable.Repeat (" ", padding));
			string uml = paddingStr + Publicity.ToUml () + name + " : " + type;
			uml += Virtuality.ToCode (" ", "");
			return uml;
		}
	}
}

