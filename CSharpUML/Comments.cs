using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUML
{
	public static class Comments
	{
		private static List<string> currentComments = new List<string> ();
		public static Dictionary<string, List<string>> CommentMap = new Dictionary<string, List<string>> ();

		public static string Key (string _name)
		{
			string dummy = "";
			string name = "";
			Packages.SplitName (_name, out dummy, out name);
			return name;
		}

		public static string Key (string _name1, string name2, string subid)
		{
			string dummy = "";
			string name1 = "";
			Packages.SplitName (_name1, out dummy, out name1);
			return name1 + "." + name2 + ":" + subid;
		}

		public static string Key (string _name1, string name2)
		{
			string dummy = "";
			string name1 = "";
			Packages.SplitName (_name1, out dummy, out name1);
			return name1 + "." + name2;
		}

		public static void AddTo (string name, string[] comments)
		{
			//foreach (string cmt in comments) { Console.WriteLine ("Comment[" + name + "] << " + cmt); }
			if (!CommentMap.ContainsKey (name))
				CommentMap [name] = new List<string> ();
			foreach (string cmt in comments) {
				if (!CommentMap [name].Contains (cmt))
					CommentMap [name].Add (cmt);
			}
		}

		public static void AddParsedComment (string line)
		{
			currentComments.Add (line);
		}

		public static string[] CurrentComments ()
		{
			string[] cmts = currentComments.ToArray ();
			currentComments = new List<string> ();
			return cmts;
		}

		public static IEnumerable<string> GetComments (string name)
		{
			if (HasComments (name))
				foreach (string cmt in CommentMap [name])
					yield return cmt;
			else
				yield break;
		}

		public static IEnumerable<string> PrintComments (string name, string padding)
		{
			if (HasComments (name)) {
				// foreach (string key in CommentMap.Keys) { Console.WriteLine (key); }
				// Console.WriteLine ("name:" + name);
				foreach (string cmt in CommentMap[name]) {
					if (cmt.Length > 0)
						Console.WriteLine ("Comment[" + name + "] = " + cmt);
					yield return padding + "// " + cmt;
				}
			} else {
				yield return padding + "// ";
			}
		}

		public static string GetCommentParameter (string key, string paramname)
		{
			foreach (string _cmt in CommentMap[key]) {
				string cmt = _cmt.TrimAll ();
				if (cmt.StartsWith ("[" + paramname) && cmt.EndsWith ("]") && cmt.Contains ("=")) {
					return cmt.Split ("=") [1].Split ("]") [0];
				}
			}
			return null;
		}

		public static IEnumerable<string> CSharpComments (string name, string padding)
		{
			if (HasComments (name)) {
				yield return padding + "/// <summary>";
				foreach (string cmt in CommentMap [name]) {
					if (cmt.Length > 0)
						Console.WriteLine ("Comment[" + name + "] = " + cmt);
					yield return padding + "/// " + cmt;
				}
				yield return padding + "/// </summary>";
			} else {
				yield break;
			}
		}

		public static bool HasComments (string name)
		{
			return name != null && CommentMap.ContainsKey (name) && CommentMap [name].Count > 0;
		}
	}
}

