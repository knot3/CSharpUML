using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUML
{
	public static class Files
	{
		public static void WriteLines (string filename, IEnumerable<string> lines)
		{
			Directory.CreateDirectory (Path.GetDirectoryName (filename));
			using (StreamWriter writer = new StreamWriter(filename)) {
				foreach (string line in lines) {
					writer.WriteLine (line);
				}
			}
		}
		
		public static IEnumerable<string> ReadLines (string filename)
		{
			using (StreamReader reader = new StreamReader(filename)) {
				while (reader.Peek() != -1) {
					yield return reader.ReadLine ();
				}
			}
		}

		public static void SearchFiles (IEnumerable<string> directories, IEnumerable<string> extensions, Action<string> add)
		{
			foreach (string directory in directories) {
				SearchFiles (directory, extensions, add);
			}
		}

		public static void SearchFiles (string directory, IEnumerable<string> extensions, Action<string> add)
		{
			Directory.CreateDirectory (directory);
			var files = Directory.GetFiles (directory, "*.*", SearchOption.AllDirectories)
				.Where (s => extensions.Any (e => s.EndsWith (e)));
			foreach (string file in files) {
				add (file);
			}
		}

		public static IEnumerable<char> ToCharacters (this IEnumerable<string> lines)
		{
			foreach (string line in lines) {
				foreach (char c in line) {
					yield return c;
				}
			}
		}
	}
}

