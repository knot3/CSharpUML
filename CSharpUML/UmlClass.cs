using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUML
{
	public class UmlClass : UmlObject, IComparable<UmlClass>
	{
		public ClassType type;
		public string[] bases;
		public IUmlObject[] Content;
		private string commentsKey;

		public UmlClass (CSharpBlock block)
			: base(block)
		{
			name = name.IfContains ("class ", () => type = ClassType.Class);
			name = name.IfContains ("struct ", () => type = ClassType.Struct);
			name = name.IfContains ("interface ", () => type = ClassType.Interface);

			string[] p = name.Split (":");
			if (p.Length == 2) {
				name = p [0].Trim ();
				bases = new HashSet<string> (p [1].Trim ().Split (",").TrimAll ()).ToArray ();
			} else {
				bases = new string[]{};
			}

			Content = ParseContent (block.Content).ToArray ();
			if (type == ClassType.Interface) {
				foreach (IUmlObject obj in Content) {
					if (obj is UmlObject)
						(obj as UmlObject).Publicity = CSharpUML.Publicity.Public;
				}
			}

			commentsKey = Comments.Key (name);
			Packages.AddToCurrentPackage (name);
		}

		public UmlClass (UmlBlock block)
			: base(block)
		{
			name = name.IfContains ("class ", () => type = ClassType.Class);
			name = name.IfContains ("struct ", () => type = ClassType.Struct);
			name = name.IfContains ("interface ", () => type = ClassType.Interface);

			string[] p = name.Split (":");
			if (p.Length == 2) {
				name = p [0].Trim ();
				bases = p [1].Trim ().Split (",").TrimAll ().ToArray ();
				if (bases.Length == 1 && bases [0].Length == 0)
					bases = new string[]{};
			} else {
				bases = new string[]{};
			}

			Content = ParseContent (block.Content).ToArray ();

			string _name = name;
			Packages.SplitName (_name, out Packages.CurrentPackage, out name);

			Comments.AddTo (commentsKey = Comments.Key (name), block.comments);
			Packages.AddToCurrentPackage (name);
		}

		public UmlClass (Tag classtag)
			: base(classtag)
		{
			if (classtag.Tagname == "interface")
				type = ClassType.Interface;
			else
				type = ClassType.Class;

			Content = ParseContent (classtag).ToArray ();
			Publicity = Publicity.Public;

			List<string> _bases = new List<string> ();
			Tag[] baseclasses = VSParser.ExtractTags (ref classtag.Content, "generalization");
			foreach (Tag baseclass in baseclasses) {
				_bases.Add (baseclass.ParseType ());
			}
			Tag[] baseinterfaces = VSParser.ExtractTags (ref classtag.Content, "interfaceRealization");
				Console.WriteLine ("interfaceRealization: " + name + ": " + classtag.Content);
			foreach (Tag baseinterface in baseinterfaces) {
				Console.WriteLine ("interfaceRealization: " + name + " <- " + baseinterface.Content);
				_bases.Add (baseinterface.ParseType ());
			}

			bases = _bases.ToArray ();

			commentsKey = Comments.Key (name);
			Packages.AddToCurrentPackage (name);
		}

		public IEnumerable<IUmlObject> ParseContent (IEnumerable<CSharpBlock> blocks)
		{
			foreach (CSharpBlock subblock in blocks) {
				if (UmlClass.Matches (subblock)) {
					yield return new UmlClass (subblock);
				} else if (UmlEnum.Matches (subblock)) {
					yield return new UmlEnum (subblock);
				} else if (UmlMethod.Matches (subblock)) {
					yield return new UmlMethod (subblock, this);
				} else if (UmlAttribute.Matches (subblock)) {
					yield return new UmlAttribute (subblock, this);
				}
			}
		}

		public IEnumerable<IUmlObject> ParseContent (IEnumerable<UmlBlock> blocks)
		{
			foreach (UmlBlock subblock in blocks) {
				if (UmlClass.Matches (subblock)) {
					yield return new UmlClass (subblock);
				} else if (UmlEnum.Matches (subblock)) {
					yield return new UmlEnum (subblock);
				} else if (UmlMethod.Matches (subblock)) {
					yield return new UmlMethod (subblock, this);
				} else if (UmlAttribute.Matches (subblock)) {
					yield return new UmlAttribute (subblock, this);
				}
			}
		}

		public IEnumerable<IUmlObject> ParseContent (Tag classtag)
		{
			Tag[] proptags = VSParser.ExtractTags (ref classtag.Content, "property");

			foreach (Tag proptag in proptags) {
				yield return new UmlAttribute (proptag, this);
			}

			Tag[] methtags = VSParser.ExtractTags (ref classtag.Content, "operation");

			foreach (Tag methtag in methtags) {
				yield return new UmlMethod (methtag, this);
			}
		}

		public static bool Matches (CSharpBlock block)
		{
			return block.Name.Contains ("class ") || block.Name.Contains ("struct ") || block.Name.Contains ("interface ");
		}

		public static bool Matches (UmlBlock block)
		{
			return block.Name.Contains ("class ") || block.Name.Contains ("struct ") || block.Name.Contains ("interface ");
		}

		public override string ToString ()
		{
			return ToUmlCode ();
		}

		public override string ToUmlCode (int padding = 0)
		{
			string paddingStr = String.Concat (Enumerable.Repeat (" ", padding));
			List<string> lines = new List<string> ();
			lines.AddRange (Comments.PrintComments (commentsKey, paddingStr));
			string nameWithPackage = Packages.IsInPackage (name) ? Packages.GetPackage (name) + "." + name : name;
			lines.Add (
				paddingStr + Publicity.ToCode ("", " ") + Virtuality.ToCode ("", " ") + type.ToCode ("", " ")
				+ nameWithPackage
				+ " : " + string.Join (", ", bases)
			);
			lines.Add (paddingStr + "  Attributes:");
			foreach (IUmlObject obj in Content) {
				if (obj is UmlAttribute) {
					lines.Add (obj.ToUmlCode (padding + 4));
				}
			}
			lines.Add (paddingStr + "  Methods:");
			foreach (IUmlObject obj in Content) {
				if (!(obj is UmlAttribute)) {
					lines.Add (obj.ToUmlCode (padding + 4));
				}
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
			lines.Add (
				paddingStr + Publicity.ToCode ("", " ") + Virtuality.ToCode ("", " ") + type.ToCode ("", " ")
				+ name
				+ (bases.Length > 0 ? " : " + string.Join (", ", bases).Replace ("XNA.", "") : "")
			);
			lines.Add (paddingStr + "{");
			lines.Add ("");

			IEnumerable<UmlAttribute> attributes = Content.OfType<UmlAttribute> ();
			IEnumerable<UmlMethod> contructors = Content.OfType<UmlMethod> ().Where ((m) => m.Name == Name);
			IEnumerable<UmlMethod> methods = Content.OfType<UmlMethod> ().Where ((m) => m.Name != Name);

			if (attributes.Count () > 0) {
				lines.Add (paddingStr + "    #region Properties");
				lines.Add ("");
				foreach (UmlAttribute obj in attributes) {
					lines.Add (obj.ToCSharpCode (padding + 4, Virtuality.Virtual, this));
					lines.Add ("");
				}
				lines.Add (paddingStr + "    #endregion");
				lines.Add ("");
			}

			if (contructors.Count () > 0) {
				lines.Add (paddingStr + "    #region Constructors");
				lines.Add ("");
				foreach (UmlMethod obj in contructors) {
					obj.IsContructor = true;
					lines.Add (obj.ToCSharpCode (padding + 4, Virtuality.None, this));
					lines.Add ("");
				}
				lines.Add (paddingStr + "    #endregion");
				lines.Add ("");
			}

			if (methods.Count () > 0) {
				lines.Add (paddingStr + "    #region Methods");
				lines.Add ("");
				foreach (UmlMethod obj in methods) {
					lines.Add (obj.ToCSharpCode (padding + 4, Virtuality.Virtual, this));
					lines.Add ("");
				}
				lines.Add (paddingStr + "    #endregion");
				lines.Add ("");
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
			if (Content.Length == 0)
				return "";

			string typestr = type == ClassType.Interface ? "Schnittstelle" : "Klasse";
			List<string> lines = new List<string> ();
			lines.Add (@"\subsubsection{" + typestr + @" " + name + @"}\index{" + Packages.GetPackage (name) + "!" + name + "}");
			
			lines.Add (@"\begin{wrapfigure}{R}{9cm}{0}" + "\n" + @"\centering");
			lines.Add (@"\includegraphics[scale=0.5]{Klassen/" + Name.Clean () + @"}");
			lines.Add (@"\end{wrapfigure}");

			lines.Add (@"\paragraph{Beschreibung:}\mbox{}\newline\newline");
			foreach (string cmt in Comments.GetComments(commentsKey)) {
				if (cmt.StartsWith ("[") && cmt.EndsWith ("]") && cmt.Contains ("="))
					continue;

				lines.Add (cmt);
			}
			//lines.Add (@"\setlength{\columnsep}{10pt}%");
			lines.Add ("\n");
			IEnumerable<UmlAttribute> attributes = Content.OfType<UmlAttribute> ();
			IEnumerable<UmlMethod> contructors = Content.OfType<UmlMethod> ().Where ((m) => m.Name == Name);
			IEnumerable<UmlMethod> methods = Content.OfType<UmlMethod> ().Where ((m) => m.Name != Name);
			if (attributes.Count () > 0) {
				lines.Add (@"\paragraph{Eigenschaften:}\mbox{} \newline\newline");
				bool first = true;
				foreach (UmlAttribute obj in attributes) {
					if (!first)
						lines.Add (@"~\\\\");
					lines.Add (obj.ToTexCode ());
					first = false;
				}
				//lines.Add (@"\end{description}");
			}
			if (contructors.Count () > 0) {
				// lines.Add (@"\paragraph{Konstruktoren:}\mbox{} \begin{description} ");
				lines.Add (@"\paragraph{Konstruktoren:}\mbox{} \newline\newline");
				bool first = true;
				foreach (UmlMethod obj in contructors) {
					if (!first)
						lines.Add (@"~\\\\");
					obj.IsContructor = true;
					lines.Add (obj.ToTexCode ());
					first = false;
				}
				//lines.Add (@"\end{description}");
			}
			if (methods.Count () > 0) {
				// lines.Add (@"\paragraph{Methoden:}\mbox{} \begin{description} ");
				lines.Add (@"\paragraph{Methoden:}\mbox{} \newline\newline");
				bool first = true;
				foreach (UmlMethod obj in methods) {
					if (!first)
						lines.Add (@"~\\\\");
					lines.Add (obj.ToTexCode ());
					first = false;
				}
				// lines.Add (@"\end{description}");
			}
			return string.Join ("\n", lines).Replace ("<", "$<$").Replace (">", "$>$");
		}

		public bool IsBase (UmlClass obj)
		{
			foreach (string baseclass in bases) {
				if (obj.Name.Clean () == baseclass.Clean ()) {
					return true;
				}
			}
			return false;
		}

		public IEnumerable<IUmlObject> FindBaseClasses (IEnumerable<IUmlObject> _objects)
		{
			IUmlObject[] objects = _objects.ToArray ();
			foreach (UmlClass obj in objects.OfType<UmlClass>()) {
				if (this.IsBase (obj)) {
					yield return obj;
					foreach (UmlClass baseOfObj in obj.FindBaseClasses(objects)) {
						yield return baseOfObj;
					}
				}
			}
		}

		public IEnumerable<IUmlObject> FindDerivedClasses (IEnumerable<IUmlObject> _objects)
		{
			IUmlObject[] objects = _objects.ToArray ();
			foreach (UmlClass obj in objects.OfType<UmlClass>()) {
				if (obj.IsBase (this)) {
					yield return obj;
					foreach (UmlClass baseOfObj in obj.FindDerivedClasses(objects)) {
						yield return baseOfObj;
					}
				}
			}
		}

		public IEnumerable<IUmlObject> FindRelated (IEnumerable<IUmlObject> objects)
		{
			yield return this;
			foreach (UmlClass obj in FindBaseClasses(objects)) {
				yield return obj;
			}
			foreach (UmlClass obj in FindDerivedClasses(objects)) {
				yield return obj;
			}
		}

		public int CompareTo (UmlClass y)
		{
			return base.CompareTo (y);
		}
	}

	public enum ClassType
	{
		Class,
		Struct,
		Interface
	}

	public static class UmlClassExtensions
	{
		public static string ToCode (this ClassType type, string before, string after)
		{
			switch (type) {
			case ClassType.Class:
				return before + "class" + after;
			case ClassType.Struct:
				return before + "struct" + after;
			case ClassType.Interface:
				return before + "interface" + after;
			default:
				return "";
			}
		}
	}
}

