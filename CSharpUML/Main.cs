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
					Uml2Code (extra, target.Length > 0 ? target : "codegen");
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
					target = target.Length > 0 ? target : "./Klassenuebersicht.uml";
					VisualStudio2Uml (extra, target);
					break;

				case Processing.VisualStudio2Tex:
					target = target.Length > 0 ? target : "./Klassenuebersicht.uml";
					VisualStudio2Uml (extra, target);
					Uml2Tex (new string[]{target}, "./Klassenuebersicht.gentex");
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

		private static void Uml2Code (IEnumerable<string> paths, string _target)
		{
			foreach (string path in paths) {
				string target = _target.Length > 0 ? _target + "/" : path + "/codegen/";

				Console.WriteLine (path);
				List<IUmlObject> objects = new List<IUmlObject> ();
				Action<string> processFile = (filename) => {
					IParser parser = new UmlParser ();
					objects.AddRange (parser.Parse (filename));
				};
				Files.SearchFiles (path, new string[]{".uml"}, processFile);

				foreach (IUmlObject obj in objects) {
					List<string> lines = new List<string> ();
					lines.Add (obj.ToCSharpCode () + "\n");
					string genfile = target + Packages.GetPackage (obj.Name).Replace (".", "/") + "/" + obj.Name.Clean () + ".cs";
					Console.WriteLine ("Write: " + genfile);
					Files.WriteLines (genfile, lines);
				}
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
							if (filename.Contains ("ModelDefinition") && !filename.Contains("ModelingProject1.uml")) {
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
				foreach (IUmlObject obj in objects.Where ((o) => !IsBlacklisted(o.Name))) {
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
			foreach (string package in Packages.PackageMap.Keys) {
				lines.Add (@"\section{Package " + package + @"}");
                
				lines.Add (@"\subsection{Klassen}");
				foreach (UmlClass obj in allObjects.OfType<UmlClass>().Where((c) => Packages.IsInPackage(package, c.Name))) {
					if (obj.type != ClassType.Interface) {
						lines.Add (obj.ToTexCode ());
						lines.Add ("");
					}
				}
				lines.Add (@"\subsection{Schnittstellen}");
				foreach (UmlClass obj in allObjects.OfType<UmlClass>().Where((c) => Packages.IsInPackage(package, c.Name))) {
					if (obj.type == ClassType.Interface) {
						lines.Add (obj.ToTexCode ());
						lines.Add ("");
					}
				}
				lines.Add (@"\subsection{Enumerationen}");
				foreach (UmlEnum obj in allObjects.OfType<UmlEnum>().Where((c) => Packages.IsInPackage(package, c.Name))) {
					lines.Add (obj.ToTexCode ());
					lines.Add ("");
				}
			}
			
			Files.WriteLines (target, lines);

			lines.Clear ();

			newCommand (ref lines, "CountClasses", "" + allObjects.OfType<UmlClass> ()
                .Where ((o) => o.type == ClassType.Class).Count ()
			);
			newCommand (ref lines, "CountInterfaces", "" + allObjects.OfType<UmlClass> ()
                .Where ((o) => o.type == ClassType.Interface).Count ()
			);
			newCommand (ref lines, "CountEnums", "" + allObjects.OfType<UmlEnum> ()
                .Count ()
			);
            newCommand(ref lines, "CountAll", ""
                + (allObjects.OfType<UmlClass>()
                .Where((o) => o.type == ClassType.Class).Count()
                + allObjects.OfType<UmlClass>()
                .Where((o) => o.type == ClassType.Interface).Count()
                + allObjects.OfType<UmlEnum>() /* Strukturen? */
                .Count())
            );

			Files.WriteLines (Path.GetDirectoryName (target) + "/Definitionen.gentex", lines);


			if (IsRunningOnMono ()) {
				foreach (UmlClass obj in allObjects.OfType<UmlClass>()) {
					// write class diagram
					ClassDiagram dia = new ClassDiagram (new IUmlObject[] { obj });
					string filename = Path.GetDirectoryName (target) + "/Klassen/" + obj.Name.Clean ();
					Files.WriteLines (filename + ".dot", dia.DotCode ("", "ffffff", 80));
					//GraphViz.Dot ("svg", filename + ".dot", filename + ".svg");
					//GraphViz.Dot ("jpg", filename + ".dot", filename + ".jpg");
					//GraphViz.Dot ("png", filename + ".dot", filename + ".png");
					GraphViz.Dot ("svg", filename + ".dot", filename + ".svg");
					GraphViz.Convert ("-density 100", "svg:" + filename + ".svg", filename + ".png");
				}
			}
		}

		private static void newCommand (ref List<string> lines, string cmd, string content)
		{
			lines.Add (@"\newcommand{\" + cmd + @"}{" + content + @"}");
		}

		private static bool IsBlacklisted (string name)
		{
			return name.Length == 1 || name.StartsWith ("XNA") || name.Contains ("IEnumerable")
				|| name.Contains ("IEquatable") || name.Contains ("ICloneable");
		}

		public static bool IsRunningOnMono ()
		{
			return Type.GetType ("Mono.Runtime") != null;
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