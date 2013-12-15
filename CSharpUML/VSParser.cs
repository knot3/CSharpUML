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

			/*
			content. s/[\r\n]+/ /gm;
			$content =~ s/\s+/ /gm;
			$content =~ s///gm;
			my %classes = ();
			my $class_or_interface;
			while (defined(my $classtag = extract_tag(["class","interface"], \$class_or_interface, \$content))) {
				my $params = parse_params(\$classtag);
				# foreach my $p (keys %$params) { print STDERR $p." => ".$params->{$p}."\n"; }
				my @properties = ();
				my @propertynames = ();
				while (defined(my $tag = extract_tag(["property"], undef, \$classtag))) {
					my $params = parse_params(\$tag);
					my $name = $params->{"name"};
					my $type = get_type(\$tag);
					my $prop = $type." ".$name;
					$prop .= " [".join(",",true_params($params))."]" if true_params($params);
					push @properties, $prop;
					push @propertynames, $name;
				}
				my @methods = ();
				my @methodnames = ();
				while (defined(my $tag = extract_tag(["operation"], undef, \$classtag))) {
					my $params = parse_params(\$tag);
					my @parameters = ();
					while (defined(my $tag = extract_tag(["parameter"], undef, \$classtag))) {
						my $params = parse_params(\$tag);
						my $p = (get_type(\$tag) || '') . ' ' . ($params->{"name"} || '');
						$p =~ s/^\s+//gm;
						$p =~ s/\s+$//gm;
						push @parameters, $p if $p;
					}
					my $name = $params->{"name"};
					my $type = get_type(\$tag);
					my $meth = $type." ".$name." (".join(", ", @parameters).")";
					$meth .= " [".join(",",true_params($params))."]" if true_params($params);
					$meth =~ s/^\s+//gm;
					$meth =~ s/\s+$//gm;
					push @methods, $meth;
					push @methodnames, $name;
				}

				# print STDERR $classtag."\n";

				$classes{$params->{"name"}} = {
					type => $class_or_interface,
					properties => [@properties],
					methods => [@methods],
					propertynames => [@propertynames],
					methodnames => [@methodnames]
				};
			}
			*/
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

