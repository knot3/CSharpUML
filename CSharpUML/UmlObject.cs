using System;
using System.Text;

namespace CSharpUML
{
	public abstract class UmlObject : IUmlObject
	{
		protected string name;
		
		public Publicity Publicity { get; set; }

		public Virtuality Virtuality { get; set; }

		public string Name { get { return name; } }

		public UmlObject (CSharpBlock block)
			: this (block.Name)
		{
		}

		public UmlObject (UmlBlock block)
			: this (block.Name)
		{
			name = name.IfContains ("+ ", () => Publicity = Publicity.Public);
			name = name.IfContains ("# ", () => Publicity = Publicity.Protected);
			name = name.IfContains ("- ", () => Publicity = Publicity.Private);
		}

		public UmlObject (Tag tag)
			: this (tag.Name)
		{
			Publicity = Publicity.Public;
			if (tag.Params.ContainsKey ("visibility")) {
				if (tag.Params ["visibility"].ToLower () == "private")
					Publicity = Publicity.Private;
				else if (tag.Params ["visibility"].ToLower () == "protected")
					Publicity = Publicity.Protected;
			}
		}

		private UmlObject (string _name)
		{
			name = " " + _name + " ";

			name = name.IfContains (" public ", " ", () => Publicity = Publicity.Public);
			name = name.IfContains (" protected ", " ", () => Publicity = Publicity.Protected);
			name = name.IfContains (" private ", " ", () => Publicity = Publicity.Private);

			name = name.IfContains (" virtual ", " ", () => Virtuality = Virtuality.Virtual);
			name = name.IfContains (" abstract ", " ", () => Virtuality = Virtuality.Abstract);
			name = name.IfContains (" override ", " ", () => Virtuality = Virtuality.Override);
			name = name.IfContains (" sealed ", " ", () => Virtuality = Virtuality.Sealed);
			name = name.IfContains (" new ", " ", () => Virtuality = Virtuality.New);
			name = name.IfContains (" static ", " ", () => Virtuality = Virtuality.Static);

			name = name.TrimAll ();
		}

		public abstract string ToUmlCode (int padding = 0);

		public abstract string ToCSharpCode (int padding = 0);

		public abstract string ToTexCode ();

		public override bool Equals (object obj)
		{
			return Equals (obj as IUmlObject);
		}

		public bool Equals (IUmlObject obj)
		{
			if (obj == null)
				return false;

			return Name == obj.Name;
		}
 
		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public int CompareTo (IUmlObject y)
		{
			return String.Compare (name, y.Name);
		}

		public static string[] TexHeader = new string[]{
			@"\newcommand{\property}[1]{\texttt{#1}}" + "\n",
			@"\newcommand{\method}[1]{\texttt{#1}}" + "\n",
			@"\newcommand{\keyword}[1]{\textcolor{BlueViolet}{#1}}" + "\n",
			@"\newcommand{\ptype}[1]{\textcolor{OliveGreen}{#1}}" + "\n",
			@"\newcommand{\varname}[1]{\textcolor{Black}{#1}}" + "\n",
		};
	}

	public enum Publicity
	{
		Private = 0,
		Protected,
		Public,
	}

	public enum Virtuality
	{
		None = 0,
		Virtual,
		Abstract,
		Override,
		Sealed,
		New,
		Static
	}

	public static class UmlObjectExtensions
	{
		public static string ToCode (this Publicity publicity, string before, string after)
		{
			switch (publicity) {
			case Publicity.Private:
				return before + "private" + after;
			case Publicity.Protected:
				return before + "protected" + after;
			case Publicity.Public:
				return before + "public" + after;
			default:
				return "";
			}
		}

		public static string ToUml (this Publicity publicity)
		{
			switch (publicity) {
			case Publicity.Protected:
				return "# ";
			case Publicity.Public:
				return "+ ";
			default:
				return "- ";
			}
		}

		public static string ToCode (this Virtuality virtuality, string before, string after)
		{
			switch (virtuality) {
			case Virtuality.Virtual:
				return before + "virtual" + after;
			case Virtuality.Abstract:
				return before + "abstract" + after;
			case Virtuality.Override:
				return before + "override" + after;
			case Virtuality.Sealed:
				return before + "sealed" + after;
			case Virtuality.New:
				return before + "new" + after;
			case Virtuality.Static:
				return before + "static" + after;
			default:
				return "";
			}
		}

		public static string CleanGenerics (this string str)
		{
			var result = new StringBuilder (str.Length);
			int inBrackets = 0;
			for (int i = 0; i < str.Length; i++) {
				if (str [i] == '<') {
					++inBrackets;
				} else if (str [i] == '>') {
					--inBrackets;
				}
				if (inBrackets > 0 && str [i] == ' ') {
					// ignore
				} else {
					result.Append (str [i]);
				}
			}
			return result.ToString ();
		}

	}
}

