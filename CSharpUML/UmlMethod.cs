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

			commentsKey = Comments.Key (classobj.Name, name, parameters.Unique () + returntype);
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

			Comments.AddTo (commentsKey = Comments.Key (classobj.Name, name, parameters.Unique () + returntype),
			                block.comments);
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

			commentsKey = Comments.Key (classobj.Name, name, parameters.Unique () + returntype);
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
			string uml = paddingStr;

			// public, virtual
			string _keywords = Comments.GetCommentParameter (commentsKey, "keywords");
			if (_keywords != null)
				uml += _keywords.TrimAll ().ToCode ("", " ");
			else if (inClass != null && inClass.type == ClassType.Interface)
				uml += "";
			else
				uml += Publicity.ToCode ("", " ") + virt.ToCode ("", " ");

			// return type
			string _returntype = Comments.GetCommentParameter (commentsKey, "returntype");
			if (_returntype != null)
				uml += _returntype.ToSharpType () + " ";
			else if (IsContructor)
				uml += "";
			else if (returntype.Length > 0)
				uml += returntype.ToSharpType ().ToCode ("", " ");
			else
				uml += "void ";

			// name
			string _name = Comments.GetCommentParameter (commentsKey, "name");
			if (_name != null)
				uml += _name;
			else
				uml += name;

			// index operator [ ]
			if (name == "this") {
				uml += " [" + string.Join (", ", parameters) + "]";
				lines.Add (uml);
				lines.Add (paddingStr + "{");
				lines.Add (paddingStr + "    " + "get { throw new System.NotImplementedException(); }");
				lines.Add (paddingStr + "    " + "set { throw new System.NotImplementedException(); }");
				lines.Add (paddingStr + "}");
			}
			// normal method
			else {
				uml += " (";
				string _parameters = Comments.GetCommentParameter (commentsKey, "parameters");
				if (_parameters != null) {
					uml += _parameters;
				} else {
					for (int i = 0; i < parameters.Length; ++i) {
						if (i > 0)
							uml += ", ";
						if (parameters [i].Contains (" ")) {
							String[] p = parameters [i].Split (new char[] { ' ' }, 2);
							uml += p [0].ToSharpType () + " " + p [1];
						} else
							uml += parameters [i].ToSharpType () + " " + parameters [i].ToLower ();
					}
				}
				uml += ")";
				if (uml.Contains ("ModelFactory") && uml.Contains ("Func<"))
					uml = paddingStr + "public ModelFactory (Func<GameScreen, GameModelInfo, GameModel> createModel)";

				string _base = Comments.GetCommentParameter (commentsKey, "base");
				if (_base != null)
					uml += "\n" + paddingStr + "    : base(" + _base.TrimAll () + ")";
			
				if (inClass.type == ClassType.Interface) {
					lines.Add (uml + ";");
				} else {
					lines.Add (uml);
					lines.Add (paddingStr + "{");
					lines.Add (paddingStr + "    " + "throw new System.NotImplementedException();");
					lines.Add (paddingStr + "}");
				}
			}
			return string.Join ("\n", lines);
		}

		public override string ToTexCode ()
		{
			List<string> lines = new List<string> ();
			string uml = Publicity.ToCode (@"\keyword{", "} ").Replace ("public ", "")
				+ Virtuality.ToCode (@"\keyword{", "} ")
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
			// lines.Add (@"\item[" + uml + @"] \item[]");
			lines.Add (@"\textbf{" + uml + @"}\newline\newline");
			foreach (string _cmt in Comments.GetComments(commentsKey)) {
				string cmt = _cmt;
				for (int i = 0; i < parameters.Length; ++i) {
					string[] parts = parameters [i].Split (new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length > 1) {
						cmt = cmt.Replace (parts [1], @"\param{" + parts [1] + @"}");
					}
				}
				lines.Add (cmt);
			}
			return string.Join ("\n", lines);
		}
	}
}

