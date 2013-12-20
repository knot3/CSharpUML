using System;
using System.Linq;
using System.Collections.Generic;

namespace CSharpUML
{
	public class UmlAttribute : UmlObject
	{
		public string type;
		private string commentsKey;

		public UmlAttribute (CSharpBlock block, UmlClass classobj)
			: base(block)
		{
			name = name.Split ('=') [0].TrimAll ();

			if (name.Contains (" ")) {
				string[] p = name.CleanGenerics ()
					.Split (new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
				type = p [0];
				name = "";
				for (int i = 0; i < p.Length; ++i) {
					name += i == 0 ? "" : " " + p [i];
				}
			}
			name = name.TrimAll ();

			commentsKey = Comments.Key (classobj.Name, name);
		}

		public UmlAttribute (UmlBlock block, UmlClass classobj)
			: base(block)
		{
			if (name.Contains (":")) {
				string[] p = name.Split (":").TrimAll ().ToArray ();
				name = p [0];
				type = p [1];
			}

			Comments.AddTo (commentsKey = Comments.Key (classobj.Name, name), block.comments);
		}

		public UmlAttribute (Tag tag, UmlClass classobj)
			: base(tag)
		{
			type = tag.ParseType ();

			commentsKey = Comments.Key (classobj.Name, name);
		}

		public static bool Matches (CSharpBlock block)
		{
			string line = block.Name;

			// index operator?
			if (line.Contains ("this [") && line.Contains ("]")) {
				return false;
			}

			// normal method?
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

			// index operator?
			if (line.Contains ("this [") && line.Contains ("]")) {
				return false;
			}

			// normal method?
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
			List<string> lines = new List<string> ();
			lines.AddRange (Comments.PrintComments (commentsKey, paddingStr));
			string uml = paddingStr + Publicity.ToUml () + name + " : " + type;
			uml += Virtuality.ToCode (" ", "");
			lines.Add (uml);
			return string.Join ("\n", lines);
		}

		public override string ToCSharpCode (int padding = 0)
		{
			return ToCSharpCode (padding, Virtuality.None, null);
		}

		public string ToCSharpCode (int padding, Virtuality virt, UmlClass inClass)
		{
			if (virt == CSharpUML.Virtuality.None)
				virt = Virtuality;
			string paddingStr = String.Concat (Enumerable.Repeat (" ", padding));
			List<string> lines = new List<string> ();
			lines.AddRange (Comments.CSharpComments (commentsKey, paddingStr));
			string uml = paddingStr
				+ ((inClass != null && inClass.type == ClassType.Interface)
                ? ""
                : Publicity.ToCode ("", " ") + Virtuality.ToCode ("", " "));
			uml += type.ToSharpType () + " " + name + " { get; set; }";
			lines.Add (uml);
			return string.Join ("\n", lines);
		}

		public override string ToTexCode ()
		{
			List<string> lines = new List<string> ();
			string uml = Publicity.ToCode (@"\keyword{", "} ").Replace ("public ", "")
				+ Virtuality.ToCode (@"\keyword{", "} ").Replace ("public ", "")
				+ @"\ptype{" + type + @"} \varname{" + name + "}";
			//lines.Add (@"\item[" + uml + @"] \item[]");
			lines.Add (@"{\textbf{" + uml + @"}\newline\newline");
			foreach (string cmt in Comments.GetComments(commentsKey)) {
				lines.Add (cmt); // + @"\\");
			}
			return string.Join ("\n", lines);
		}
	}
}

