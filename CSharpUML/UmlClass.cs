using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUML
{
	public class UmlClass : UmlObject
	{
		public ClassType type;
		public string[] bases;
		public IUmlObject[] Content;

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
		}

		public IEnumerable<IUmlObject> ParseContent (IEnumerable<CSharpBlock> blocks)
		{
			foreach (CSharpBlock subblock in blocks) {
				if (UmlClass.Matches (subblock)) {
					yield return new UmlClass (subblock);
				} else if (UmlEnum.Matches (subblock)) {
					yield return new UmlEnum (subblock);
				} else if (UmlMethod.Matches (subblock)) {
					yield return new UmlMethod (subblock);
				} else if (UmlAttribute.Matches (subblock)) {
					yield return new UmlAttribute (subblock);
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
					yield return new UmlMethod (subblock);
				} else if (UmlAttribute.Matches (subblock)) {
					yield return new UmlAttribute (subblock);
				}
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
			lines.Add (
				paddingStr + Publicity.ToCode ("", " ") + Virtuality.ToCode ("", " ") + type.ToCode ("", " ")
				+ name
				+ " : " + string.Join (", ", bases)
			);
			foreach (IUmlObject obj in Content) {
				lines.Add (obj.ToUmlCode (padding + 4));
			}
			return string.Join ("\n", lines);
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

