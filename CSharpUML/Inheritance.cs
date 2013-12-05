using System;
using System.Collections.Generic;
using System.Linq;

using NDesk.Options;

namespace CSharpUML
{
	public class Inheritance
	{
		public IUmlObject Base { get; set; }

		public IUmlObject Derived { get; set; }

		public Inheritance (IUmlObject baseobj, IUmlObject derivedobj)
		{
			Base = baseobj;
			Derived = derivedobj;
		}

		public static void From (IUmlObject[] objects, out List<Inheritance> inhs, out List<string> unknownObjects)
		{
			Dictionary<string, IUmlObject> names = new Dictionary<string, IUmlObject> ();

			foreach (IUmlObject obj in objects) {
				names [obj.Name.Clean ().ToLower ()] = obj;
			}

			inhs = new List<Inheritance> ();
			unknownObjects = new List<string> ();

			foreach (IUmlObject obj in objects) {
				if (obj is UmlClass) {
					UmlClass cl = obj as UmlClass;
					foreach (string basename in cl.bases) {
						// Console.WriteLine("basename="+basename);
						IUmlObject baseobj;
						if (names.ContainsKey (basename.Clean ().ToLower ())) {
							baseobj = names [basename.Clean ().ToLower ()];
						} else {
							baseobj = new UmlClass (new UmlBlock (name: "+ " + basename));
							unknownObjects.Add (basename);
						}
						Inheritance inh = new Inheritance (baseobj: baseobj, derivedobj: obj);
						inhs.Add (inh);
					}
				}
			}
		}
	}
}