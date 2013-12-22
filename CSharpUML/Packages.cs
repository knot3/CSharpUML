using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpUML
{
	public static class Packages
	{
		public static string CurrentPackage = "";
		public static Dictionary<string, List<string>> PackageMap = new Dictionary<string, List<string>> ();

		public static void AddToPackage (string package, string name)
		{
			if (!PackageMap.ContainsKey (package))
				PackageMap [package] = new List<string> ();
			if (!PackageMap [package].Contains (name))
				PackageMap [package].Add (name);
		}

		public static void AddToCurrentPackage (string name)
		{
			if (CurrentPackage.Length > 0)
				AddToPackage (CurrentPackage, name);
		}

		public static bool IsInPackage (string package, string name)
		{
			List<string> list = PackageMap [package];
			Console.WriteLine ("package: " + package + " => " + string.Join (", ", list.ToArray ()));
			if (list.Contains (name)) {
				return true;
			}
			return false;
		}

		public static bool IsInPackage (string name)
		{
			foreach (string package in PackageMap.Keys) {
				List<string> list = PackageMap [package];
				Console.WriteLine ("package: " + package + " => " + string.Join (", ", list.ToArray ()));
				if (list.Contains (name)) {
					return true;
				}
			}
			return false;
		}

		public static string GetPackage (string name)
		{
			foreach (string package in PackageMap.Keys) {
				List<string> list = PackageMap [package];
				if (list.Contains (name)) {
					return package;
				}
			}
			return "";
		}

		public static string AddTexCommands (string comment)
		{
			foreach (string package in PackageMap.Keys) {
				foreach (string classname in PackageMap[package]) {
					Regex rgx = new Regex (@"([^a-zA-Z0-9])(" + classname.Clean () + @")([^a-zA-Z0-9])");
					if (classname.StartsWith ("I"))
						comment = rgx.Replace (comment, @"$1\interface{$2}$3");
					else
						comment = rgx.Replace (comment, @"$1\class{$2}$3");
				}
			}
			return comment;
		}

		public static void SplitName (string fullname, out string package, out string name)
		{
			if (fullname.Contains (".")) {
				string[] nparts = fullname.Split ('.');
				package = "";
				name = "";
				foreach (string npart in nparts) {
					if (npart.Length > 0) {
						if (package.Length > 0)
							package += ".";
						package += name;
						name = npart;
					}
				}
			} else {
				name = fullname;
				package = "";
			}
		}

		public static IEnumerable<string> GetUsingStatements (params string[] _exclude)
		{
			HashSet<string> exclude = new HashSet<string> (_exclude);
			string[] dotnet = new string[] {
				"System",
				"System.Collections",
				"System.Collections.Generic",
				"System.Linq"
			};
			string[] xna = new string[]{
				"Microsoft.Xna.Framework", "Microsoft.Xna.Framework.Audio", "Microsoft.Xna.Framework.Content",
				"Microsoft.Xna.Framework.GamerServices", "Microsoft.Xna.Framework.Graphics",
				"Microsoft.Xna.Framework.Input", "Microsoft.Xna.Framework.Media", "Microsoft.Xna.Framework.Net",
				"Microsoft.Xna.Framework.Storage"
			};
			
			foreach (string pkg in dotnet) {
				if (!exclude.Contains (pkg))
					yield return "using " + pkg + ";";
			}
			yield return "";
			foreach (string pkg in xna) {
				if (!exclude.Contains (pkg))
					yield return "using " + pkg + ";";
			}
			yield return "";
			foreach (string pkg in PackageMap.Keys) {
				if (!exclude.Contains (pkg))
					yield return "using " + pkg + ";";
			}
			yield return "";
		}
	}
}

