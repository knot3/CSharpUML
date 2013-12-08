using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NDesk.Options;

namespace CSharpUML
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			Processing processing = Processing.None;
			bool help = false;
			string target = "";
			int verbose = 0;
			var p = new OptionSet () {
				{ "v|verbose",			v => ++verbose },
				{ "h|?|help",			v => help = v != null },
				{ "file=",				v => {} },
				{ "target=",			v => target = v },
				{ "u|code2uml",			v => processing = Processing.CodeToUml },
				{ "c|uml2code",			v => processing = Processing.UmlToCode },
				{ "d|uml2diagram",		v => processing = Processing.UmlToDiagram }
			};

			List<string> extra = p.Parse (args);
			if (extra.Count == 0)
				extra.Add (".");

			if (processing == Processing.None)
				processing = Processing.CodeToDiagram;
			Console.WriteLine (processing.ToString () + "...");

			if (help) {
			} else {
				switch (processing) {

				case Processing.CodeToUml:
					// c# code -> uml code
					Code2Uml (extra, target.Length > 0 ? target : "");
					break;

				case Processing.UmlToCode:
					// uml code -> c# code
					Uml2Code (extra, target.Length > 0 ? target : "gen");
					break;

				case Processing.UmlToDiagram:
					// uml code -> dot code
					Uml2Diagram (extra, target.Length > 0 ? target : "graphs");
					break;

				case Processing.CodeToDiagram:
					// c# code -> dot code
					Code2Uml (extra, "");
					Uml2Diagram (extra, target.Length > 0 ? target : "graphs");
					break;
				}
			}
		}

		private static void Code2Uml (IEnumerable<string> paths, string target)
		{
			foreach (string path in paths) {
				Console.WriteLine (path);
				Action<string> processFile = (filename) => {
					if (!filename.Contains ("gen")) {
						IParser parser = new CSharpParser ();
						IEnumerable<IUmlObject> objects = parser.Parse (filename);
						List<string> lines = new List<string> ();
						foreach (IUmlObject obj in objects) {
							lines.Add (obj.ToUmlCode ());
						}
						string umlfile = filename.Replace (".cs", ".uml");
						if (target.Length > 0) {
							umlfile = umlfile.ReplaceFirst (path, target + "/");
						}
						Console.WriteLine ("Write: " + umlfile);
						Files.WriteLines (umlfile, lines);
					}
				};
				Files.SearchFiles (path, new string[]{".cs"}, processFile);
			}
		}

		private static void Uml2Code (IEnumerable<string> paths, string target)
		{
			foreach (string path in paths) {
				Console.WriteLine (path);
				Action<string> processFile = (filename) => {
					IParser parser = new UmlParser ();
					IEnumerable<IUmlObject> objects = parser.Parse (filename);
					List<string> lines = new List<string> ();
					foreach (IUmlObject obj in objects) {
						lines.Add (obj.ToUmlCode () + "\n");
					}
					string genfile = filename.Replace (".uml", ".cs")
								.Replace ("/uml/", "/");
					if (target.Length > 0) {
						genfile = genfile.ReplaceFirst (path, target + "/");
					} else {
						genfile = genfile.ReplaceFirst (path, path + "/gen/");
					}
					Console.WriteLine ("Write: " + genfile);
					Files.WriteLines (genfile, lines);
				};
				Files.SearchFiles (path, new string[]{".uml"}, processFile);
			}
		}

		private static void Uml2Diagram (IEnumerable<string> paths, string target)
		{
			foreach (string path in paths) {
				Console.WriteLine (path);
				string graphdir = target.Length > 0 ? target + "/" : path + "/graphs/";

				List<IUmlObject> allObjects = new List<IUmlObject> ();
				Action<string> processFile = (filename) => {
					if (!filename.Contains ("graphs")) {
						IParser parser = new UmlParser ();
						Console.WriteLine ("Read: " + filename);
						allObjects.AddRange (parser.Parse (filename));
						//foreach (IUmlObject obj in objects) {
						//	Console.WriteLine (obj);
						//}
					}
				};
				Files.SearchFiles (path, new string[]{".uml"}, processFile);

				Console.WriteLine ("Write: " + "classes.dot");
				ClassDiagram allDia = new ClassDiagram (allObjects);
				Files.WriteLines (graphdir + "classes.dot", allDia.DotCode ());
				GraphViz.Dot ("svg", graphdir + "classes.dot", graphdir + "classes.svg");
				GraphViz.Dot ("png", graphdir + "classes.dot", graphdir + "classes.png");

				foreach (UmlClass obj in allObjects.OfType<UmlClass>()) {
					string filename = "class-" + obj.Name.Clean ();
					Console.WriteLine ("Write: " + filename + ".svg");
					
					IEnumerable<IUmlObject> relatedObjects = obj.FindRelated (allObjects);

					// write class diagram
					ClassDiagram dia = new ClassDiagram (relatedObjects);
					Files.WriteLines (graphdir + filename + ".dot", dia.DotCode ());
					GraphViz.Dot ("svg", graphdir + filename + ".dot", graphdir + filename + ".svg");

					// write uml code
					List<string> lines = new List<string> ();
					foreach (IUmlObject relObj in relatedObjects) {
						lines.Add (relObj.ToUmlCode () + "\n");
					}
					Files.WriteLines (graphdir + filename + ".uml", lines);
				}
			}
		}
	}

	enum Processing
	{
		None = 0,
		CodeToUml,
		UmlToCode,
		UmlToDiagram,
		CodeToDiagram
	}
}