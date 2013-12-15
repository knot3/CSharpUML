using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NDesk.Options;
using System.IO;

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
				{ "target=",			v => target = v },
				{ "code2uml",			v => processing = Processing.CodeToUml },
				{ "code2diagram",		v => processing = Processing.CodeToDiagram },
				{ "uml2code",			v => processing = Processing.UmlToCode },
				{ "uml2diagram",		v => processing = Processing.UmlToDiagram },
				{ "vs2uml",				v => processing = Processing.VisualStudio2Uml },
				{ "vs2tex",				v => processing = Processing.VisualStudio2Tex },
			};

			List<string> extra = p.Parse (args);
			if (extra.Count == 0)
				extra.Add (".");

			if (processing == Processing.None)
				processing = Processing.VisualStudio2Tex;
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

				case Processing.VisualStudio2Uml:
					target = target.Length > 0 ? target : "./Klassenindex.uml";
					VisualStudio2Uml (extra, target);
					break;

				case Processing.VisualStudio2Tex:
					target = target.Length > 0 ? target : "./Klassenindex.uml";
					VisualStudio2Uml (extra, target);
					Uml2Tex (new string[]{target}, "./Klassenindex.gentex");
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
					if (!filename.Contains ("graphs") && !filename.Contains ("ModelDefinition")) {
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

		private static void VisualStudio2Uml (IEnumerable<string> paths, string target)
		{
			if (target.Length > 0) {
				try {
					Console.WriteLine ("Read: " + target);
					IUmlObject[] readObjs = new UmlParser ().Parse (target).ToArray ();
					Console.WriteLine ("Read: " + target);
				} catch (FileNotFoundException ex) {
					Console.WriteLine (ex.ToString ());
				}

				List<IUmlObject> objects = new List<IUmlObject> ();
				for (int _try = 0; _try <= 3 && objects.Count == 0; ++_try) {
					foreach (string _path in paths) {
						string path = _path;
						for (int p = 0; p < _try; ++p)
							path += "/../";
						Console.WriteLine (path);
						Action<string> processFile = (filename) => {
							if (filename.Contains ("ModelDefinition")) {
								Console.WriteLine ("Read: " + filename);
								IParser parser = new VSParser ();
								objects.AddRange (parser.Parse (filename));
							}
						};
						Files.SearchFiles (path, new string[]{".uml"}, processFile);
					}
				}
				objects.Sort ();
				Console.WriteLine ("Write: " + target);
				List<string> lines = new List<string> ();
				foreach (IUmlObject obj in objects) {
					lines.Add (obj.ToUmlCode ());
					lines.Add ("");
				}
				Files.WriteLines (target, lines);
			} else {
				Console.WriteLine ("Error! No target file specified!");
			}
		}

		private static void Uml2Tex (IEnumerable<string> umlfiles, string target)
		{
			List<IUmlObject> allObjects = new List<IUmlObject> ();
			foreach (string umlfile in umlfiles) {
				IParser parser = new UmlParser ();
				Console.WriteLine ("Read: " + umlfile);
				allObjects.AddRange (parser.Parse (umlfile));
			}

			Console.WriteLine ("Write: " + target);
			List<string> lines = new List<string> ();
			lines.AddRange (UmlObject.TexHeader);
			foreach (IUmlObject obj in allObjects) {
				lines.Add (obj.ToTexCode ());
				lines.Add ("");
			}
			Files.WriteLines (target, lines);

			foreach (UmlClass obj in allObjects.OfType<UmlClass>()) {
				// write class diagram
				ClassDiagram dia = new ClassDiagram (new IUmlObject[]{ obj });
				string filename = Path.GetDirectoryName (target) + "/Klassen/" + obj.Name.Clean ();
				Files.WriteLines (filename + ".dot", dia.DotCode ());
				//GraphViz.Dot ("svg", filename + ".dot", filename + ".svg");
				//GraphViz.Dot ("jpg", filename + ".dot", filename + ".jpg");
				GraphViz.Dot ("png", filename + ".dot", filename + ".png");
			}
		}
	}

	enum Processing
	{
		None = 0,
		CodeToUml,
		UmlToCode,
		UmlToDiagram,
		CodeToDiagram,
		VisualStudio2Uml,
		VisualStudio2Tex,
	}
}