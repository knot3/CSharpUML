using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpUML
{
	public static class Strings
	{
		public static string[] Split (this string s, string separator)
		{
			return s.Split (new string[] { separator }, StringSplitOptions.None);
		}

		public static string TrimAll (this string s)
		{
			return s.Trim (' ', '\r', '\n', '\t');
		}

		public static string ToCSharpType (this string s)
		{
			return s.Replace ("Integer", "int").Replace ("String", "string")
                .Replace ("XNA.", "");
		}

		public static IEnumerable<string> TrimAll (this IEnumerable<string> strings)
		{
			foreach (string s in strings) {
				yield return s.TrimAll ();
			}
		}

		public static string ReplaceFirst (this string text, string search, string replace)
		{
			int pos = text.IndexOf (search);
			if (pos < 0) {
				return text;
			}
			return text.Substring (0, pos) + replace + text.Substring (pos + search.Length);
		}

		public static int Indentation (this string text)
		{
			int indent = 0;
			for (int i = 0; i < text.Length; ++i) {
				if (text [i] == ' ')
					++indent;
				else
					break;
			}
			return indent;
		}

		public static string IfContains (this string str, string find, string replacement, Action action)
		{
			if (str.Contains (find)) {
				action ();
			}
			return str.Replace (find, replacement);
		}

		public static string IfContains (this string str, string find, Action action)
		{
			return str.IfContains (find, "", action);
		}

		public static string Clean (this string str)
		{
			return new String (str.Split ("<") [0].Where (Char.IsLetterOrDigit).ToArray ());
		}

		public static string Unique (this string[] arr)
		{
			string str = "";
			foreach (string elem in arr) {
				str += elem.Clean ().ToLower ();
			}
			return str;
		}

		public static string ToTexCode (this string str)
		{
			if (str.StartsWith ("_"))
				str = str.Substring (1);
			return str;
		}

		public static string RegexReplace (this string str, string find, string replacement)
		{
			return Regex.Replace (str, find, replacement);
		}

		public static string ToCode (this string str, string before, string after)
		{
			if (str.Length > 0)
				return before + str + after;
			else
				return str;
		}
	}
}