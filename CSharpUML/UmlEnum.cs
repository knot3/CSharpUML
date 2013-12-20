using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUML
{
	public class UmlEnum : UmlObject
	{
		private string[] values;
		private string commentsKey;

		public UmlEnum (CSharpBlock block)
			: base(block)
		{
			name = name.IfContains ("enum ", () => {});
			values = ParseContent (block.Content).ToArray ();

			commentsKey = Comments.Key (name);
			Packages.AddToCurrentPackage (name);
		}

		public UmlEnum (UmlBlock block)
			: base(block)
		{
			name = name.IfContains ("enum ", () => {});
			values = ParseContent (block.Content).ToArray ();
			
			string _name = name;
			Packages.SplitName (_name, out Packages.CurrentPackage, out name);

			Comments.AddTo (commentsKey = Comments.Key (name), block.comments);
			Packages.AddToCurrentPackage (name);
		}

		public UmlEnum (Tag tag)
			: base(tag)
		{
			name = tag.Params ["name"];

			tag.Content = tag.Content.Replace (" = ", "=");
			Tag[] literalTags = VSParser.ExtractTags (ref tag.Content, "enumerationLiteral");

			List<string> literals = new List<string> ();
			foreach (Tag literalTag in literalTags) {
				if (literalTag.Params.ContainsKey ("name")) {
					literals.Add (literalTag.Params ["name"]);
				} else {
					Console.WriteLine ("weird: " + literalTag.Content);
				}
			}
			values = literals.ToArray ();

			commentsKey = Comments.Key (name);
			Packages.AddToCurrentPackage (name);
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
				string literal = subblock.Name.TrimAll ();
				Comments.AddTo (commentsKey = Comments.Key (name, literal), subblock.comments);
				yield return literal;
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
			lines.AddRange (Comments.PrintComments (commentsKey, paddingStr));
			string nameWithPackage = Packages.IsInPackage (name) ? Packages.GetPackage (name) + "." + name : name;
			lines.Add (paddingStr + Publicity.ToCode ("", " ") + Virtuality.ToCode ("", " ") + "enum " + nameWithPackage);
			foreach (string literal in values) {
				lines.AddRange (Comments.PrintComments (Comments.Key (name, literal), paddingStr + "    "));
				lines.Add (paddingStr + "    " + literal);
			}
			return string.Join ("\n", lines);
		}

		public override string ToCSharpCode (int padding = 0)
		{
			string paddingStr = String.Concat (Enumerable.Repeat (" ", padding));
			List<string> lines = new List<string> ();

			lines.AddRange (Packages.GetUsingStatements (Packages.GetPackage (name)));
			
			if (Packages.IsInPackage (name)) {
				lines.Add (paddingStr + "namespace " + Packages.GetPackage (name));
				lines.Add (paddingStr + "{");
				paddingStr += "    ";
				padding += 4;
			}

			lines.AddRange (Comments.CSharpComments (commentsKey, paddingStr));
			lines.Add (paddingStr + Publicity.ToCode ("", " ") + Virtuality.ToCode ("", " ") + "enum " + name);
			lines.Add (paddingStr + "{");
			foreach (string literal in values) {
				lines.AddRange (Comments.CSharpComments (Comments.Key (name, literal), paddingStr + "    "));
				lines.Add (paddingStr + "    " + literal + ",");
			}
			lines.Add (paddingStr + "}");

			if (Packages.IsInPackage (name)) {
				padding -= 4;
				paddingStr = paddingStr.Substring (4);
				lines.Add (paddingStr + "}");
			}

			return string.Join ("\n", lines);
		}

		public override string ToTexCode ()
		{
			List<string> lines = new List<string> ();
			lines.Add (@"\subsubsection{Enumeration " + name + @"}");
            lines.Add(@"\paragraph{Beschreibung:}\mbox{}\newline\newline");
			foreach (string cmt in Comments.GetComments(commentsKey)) {
				lines.Add (cmt);
			}
			lines.Add ("\n");
			lines.Add (@"\paragraph{Eigenschaften:}\mbox{} \newline\newline");
			foreach (string _literal in values) {
				string literal = _literal;
				if (literal.Contains ("=")) {
					string[] p = literal.Split (new char[]{'='}, 2, StringSplitOptions.RemoveEmptyEntries);
					literal = @"\ptype{" + p [0] + @"} \keyword{ = } \varname{" + p [1] + @"}";
				} else {
					literal = @"\ptype{" + literal + "}";
				}
                lines.Add(@"\textbf{" + literal + @"}\newline\newline");
				foreach (string cmt in Comments.GetComments(Comments.Key(name, _literal))) {
					lines.Add (cmt);
				}
				lines.Add (@"~\\\\");
			}
			return string.Join ("\n", lines);
		}
	}

}

