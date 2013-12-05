using System;
using System.Linq;

namespace CSharpUML
{
	public class UmlMethod : UmlObject
	{
		public string[] parameters;
		public string returntype;

		public UmlMethod (CSharpBlock block)
			: base(block)
		{
			int indexBracketOpen = name.IndexOf ("(");
			int indexBracketClose = name.IndexOf (")");
			string paramStr = name.Substring (indexBracketOpen + 1, indexBracketClose - indexBracketOpen - 1);
			parameters = paramStr.Split (',').TrimAll ().ToArray ();
			name = name.IfContains ("(" + paramStr + ")", () => {});
			if (name.Contains (" ")) {
				string[] p = name.Split (new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
				returntype = p [0];
				name = "";
				for (int i = 0; i < p.Length; ++i) {
					name += i == 0 ? "" : " " + p [i];
				}
			}
			if (name == "") {
				name = returntype;
				returntype = "";
			}
			if (returntype == "void")
				returntype = "";
			name = name.TrimAll ();
		}

		public UmlMethod (UmlBlock block)
			: base(block)
		{
			int indexBracketOpen = name.IndexOf ("(");
			int indexBracketClose = name.IndexOf (")");
			string paramStr = name.Substring (indexBracketOpen + 1, indexBracketClose - indexBracketOpen - 1);
			parameters = paramStr.Split (',').TrimAll ().ToArray ();
			name = name.IfContains ("(" + paramStr + ")", () => {});

			if (name.Contains (":")) {
				string[] p = name.Split (":").TrimAll ().ToArray ();
				name = p [0];
				returntype = p [1];
			} else {
				returntype = "";
			}
		}

		public static bool Matches (CSharpBlock block)
		{
			string line = block.Name;
			int indexBracketOpen = line.IndexOf ("(");
			int indexBracketClose = line.IndexOf (")");
			int indexEqualSign = line.IndexOf ("=");
			if (indexBracketOpen != -1 && indexBracketClose != -1) {
				if (indexEqualSign == -1) {
					return true;
				} else if (indexBracketOpen < indexEqualSign && indexBracketClose > indexEqualSign) {
					return true;
				}
			}
			return false;
		}

		public static bool Matches (UmlBlock block)
		{
			return block.Name.Contains (") :");
		}

		public override string ToUmlCode (int padding = 0)
		{
			string paddingStr = String.Concat (Enumerable.Repeat (" ", padding));
			string uml = paddingStr + Publicity.ToUml () + name + " (" +
				string.Join (", ", parameters) + ")";
			if (returntype.Length > 0)
				uml += " : " + returntype;
			uml += Virtuality.ToCode (" ", "");
			return uml;
		}
	}
}

