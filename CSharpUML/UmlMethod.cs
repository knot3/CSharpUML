using System;
using System.Linq;
using System.Collections.Generic;

namespace CSharpUML
{
	public class UmlMethod : UmlObject
	{
		public bool IsContructor = false;
		public string[] parameters;
		public string returntype;
		private string commentsKey;

		public UmlMethod (CSharpBlock block, UmlClass classobj)
			: base(block)
		{
			parseParams ();

			if (name.Contains (" ")) {
				string[] p = name.CleanGenerics ()
					.Split (new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
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

			commentsKey = Comments.Key (classobj.Name, name, parameters.Unique());
		}

		public UmlMethod (UmlBlock block, UmlClass classobj)
			: base(block)
		{
			parseParams ();

			if (name.Contains (":")) {
				string[] p = name.Split (":").TrimAll ().ToArray ();
				name = p [0];
				returntype = p [1];
			} else {
				returntype = "";
			}

			Comments.AddTo (commentsKey = Comments.Key (classobj.Name, name, parameters.Unique()), block.comments);
		}

		public UmlMethod (Tag tag, UmlClass classobj)
			: base(tag)
		{
			Tag[] paramtags = VSParser.ExtractTags (ref tag.Content, "parameter");

			returntype = "void";

			List<string> parameterlist = new List<string> ();
			foreach (Tag proptag in paramtags) {
				string type = proptag.ParseType ();
				if (proptag.Params.ContainsKey ("direction") && proptag.Params ["direction"].ToLower () == "return") {
					returntype = type;
				} else {
					if (type == "void")
						parameterlist.Add (proptag.Name);
					else
						parameterlist.Add (type + " " + proptag.Name);
				}
			}
			parameters = parameterlist.ToArray ();

			commentsKey = Comments.Key (classobj.Name, name, parameters.Unique());
		}

		private void parseParams ()
		{
			// index operator?
			if (name.Contains ("this [") && name.Contains ("]")) {
				int indexBracketOpen = name.IndexOf ("[");
				int indexBracketClose = name.IndexOf ("]");
				string paramStr = name.Substring (indexBracketOpen + 1, indexBracketClose - indexBracketOpen - 1);
				parameters = paramStr.Split (',').TrimAll ().ToArray ();
				name = name.IfContains ("[" + paramStr + "]", () => {});
			}
			// normal method?
			else {
				int indexBracketOpen = name.IndexOf ("(");
				int indexBracketClose = name.IndexOf (")");
				string paramStr = name.Substring (indexBracketOpen + 1, indexBracketClose - indexBracketOpen - 1);
				parameters = paramStr.Split (',').TrimAll ().ToArray ();
				name = name.IfContains ("(" + paramStr + ")", () => {});
			}
		}

		public static bool Matches (CSharpBlock block)
		{
			string line = block.Name;

			// index operator?
			if (line.Contains ("this [") && line.Contains ("]")) {
				return true;
			}

			// normal method?
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
			string line = block.Name;

			// index operator?
			if (line.Contains ("this [") && line.Contains ("]")) {
				return true;
			}

			// normal method?
			int indexBracketOpen = line.IndexOf ("(");
			int indexBracketClose = line.IndexOf (")");
			if (indexBracketOpen != -1 && indexBracketClose != -1)
				return true;
			else 
				return false;
		}

		public override string ToUmlCode (int padding = 0)
		{
			string paddingStr = String.Concat (Enumerable.Repeat (" ", padding));
			List<string> lines = new List<string> ();
			lines.AddRange (Comments.PrintComments (commentsKey, paddingStr));
			string uml = paddingStr + Publicity.ToUml () + name + " ("
				+ string.Join (", ", parameters) + ")";
			if (returntype.Length > 0)
				uml += " : " + returntype;
			uml += Virtuality.ToCode (" ", "");
			lines.Add (uml);
			return string.Join ("\n", lines);
		}

		public override string ToTexCode ()
		{
			List<string> lines = new List<string> ();
			string uml = Publicity.ToCode (@"\keyword{", "} ").Replace ("public ", "")
				+ Virtuality.ToCode (@"\keyword{", "} ").Replace ("public ", "")
				+ (IsContructor ? "" : @"\ptype{" + returntype + @"} ")
				+ @"\varname{" + name.ToTexCode () + "} (";
			for (int i = 0; i < parameters.Length; ++i) {
				string[] parts = parameters [i].Split (new char[]{' '}, 2, StringSplitOptions.RemoveEmptyEntries);
				if (i > 0)
					uml += ", ";
				if (parts.Length == 1)
					uml += @"\ptype{" + parts [0] + @"}";
				else if (parts.Length > 1)
					uml += @"\ptype{" + parts [0] + @"} \varname{" + parts [1].ToTexCode () + "}";
			}
			uml += ")" + Virtuality.ToCode (" ", "");
			lines.Add (@"\item[" + uml + @"] \item[]"); // \property{" + uml + @"} & ");
			foreach (string cmt in Comments.GetComments(commentsKey)) {
				lines.Add (cmt); // + @"\\");
			}
			return string.Join ("\n", lines);
		}
	}
}

