using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpUML
{
	public class VSParser : IParser
	{
		public VSParser ()
		{

		}

		public static Tag[] ExtractTags (ref string content, params string[] tagnames)
		{
			List<Tag> tags = new List<Tag> ();
			foreach (string tagname in tagnames) {
				MatchCollection matches = Regex.Matches (content, @"([<]" + tagname + @" )([^>]*?)( /[>])");

				foreach (Match match in matches) {
					string tagcontent = match.Groups [1].Value + match.Groups [2].Value + match.Groups [3].Value;
					content.Replace (tagcontent, "");
					tags.Add (new Tag (tagname: tagname, content: tagcontent));
				}

				matches = Regex.Matches (content, @"([<]" + tagname + @"[ >])([^>]*?"+"\""+@">.*?)([<]/" + tagname + @"[>])");
				Console.WriteLine ("match: " + @"([<]" + tagname + @"[ >])(.*?)([<]/" + tagname + @"[>])");
				Console.WriteLine ("matches: " + matches.Count);

				foreach (Match match in matches) {
					string tagcontent = match.Groups [1].Value + match.Groups [2].Value + match.Groups [3].Value;
					content.Replace (tagcontent, "");
					tags.Add (new Tag (tagname: tagname, content: tagcontent));
				}
			}
			return tags.ToArray ();
		}
		
		public IEnumerable<IUmlObject> Parse (IEnumerable<string> lines)
		{
			string content = String.Join (" ", lines);
			
			// Console.WriteLine ("content: " + content);
			content = content.RegexReplace (@"[\r\n\s]+", " ");
			content = content.RegexReplace ("\"........-....-....-....-............\"", "");

			Tag[] junk = ExtractTags(ref content, "redefinableTemplateSignature");

			Tag[] classes = ExtractTags (ref content, "class", "Interface");

			foreach (Tag tag in classes) {
				if (tag.Params.ContainsKey ("name")) {
					Console.WriteLine ("Found " + tag.Tagname + ": " + tag.Params ["name"]);
					yield return new UmlClass (tag);
				} else {
					Console.WriteLine ("weird: " + tag.Content);
				}
			}

			Tag[] enumerations = ExtractTags (ref content, "enumeration");

			foreach (Tag tag in enumerations) {
				if (tag.Params.ContainsKey ("name")) {
					Console.WriteLine ("Found " + tag.Tagname + ": " + tag.Params ["name"]);
					yield return new UmlEnum (tag);
				} else {
					Console.WriteLine ("weird: " + tag.Content);
				}
			}
		}

		public IEnumerable<IUmlObject> Parse (string filename)
		{
			IEnumerable<IUmlObject> parsed = Parse (Files.ReadLines (filename).TrimAll ());
			foreach (IUmlObject obj in parsed) {
				yield return obj;
			}
		}
	}

	public struct Tag
	{
		public string Tagname;
		public string Name;
		public string Content;
		public Dictionary<string, string> Params;
		public List<string> TrueParams;

		public Tag (string tagname, string content)
		{
			Tagname = tagname.ToLower ();
			Name = "";
			Content = content;
			Params = new Dictionary<string, string> ();
			TrueParams = new List<string> ();
			parseParams ();
			Name = Name.Replace ("&lt;", "<").Replace ("&gt;", ">");
		}

		private void parseParams ()
		{
			if (Content.Contains ("<")) {
				string _tagstr = Content.Split ('<') [1].Split ('>') [0];
				string tagstr = _tagstr;
				foreach (string part in tagstr.Split(" ")) {
					if (part.Contains ("=\"") && part.EndsWith ("\"")) {
						string[] _part = part.Split ("=\"");
						string key = _part [0];
						string value = _part [1].Replace ("\"", "");
						Params [key] = value;
						tagstr = tagstr.Replace (part, "");
					}
				}
				Content = Content.ReplaceFirst (_tagstr, tagstr);
			}

			if (Params.ContainsKey ("name"))
				Name = Params ["name"];

			foreach (string _key in Params.Keys) {
				if (Params [_key] == "true") {
					string key = _key.ToLower ();
					if (key.StartsWith ("is"))
						key = key.Substring (2);
					TrueParams.Add (key);
				}
			}
		}

		public string ParseType ()
		{
			string type = "void";
			if (Content.Contains ("LastKnownName=\"")) {
				type = Content.Split ("LastKnownName=\"") [1].Split ('"') [0]
					.Replace ("&lt;", "<").Replace ("&gt;", ">");
			}
			return type;
		}
	};
		
		
}

