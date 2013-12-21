using System;
using System.Diagnostics;

namespace CSharpUML
{
	public static class GraphViz
	{
		public static void Dot (string format, string input, string output, string extra="")
		{
			try {
				var info = new ProcessStartInfo (
					"dot", "-T" + format + " -o'" + output + "' '" + input + "'" //+ " -Gdpi=300"
				);
				info.WindowStyle = ProcessWindowStyle.Hidden;
				Console.Write (".");
				using (Process exeProcess = Process.Start (info)) {
					exeProcess.WaitForExit ();
				}

			} catch (Exception) {
			}
		}

		public static void Convert (params string[] parameters)
		{
			try {
				var info = new ProcessStartInfo (
  				  "convert", string.Join (" ", parameters)
				);
				info.WindowStyle = ProcessWindowStyle.Hidden;
				Console.Write (".");
				using (Process exeProcess = Process.Start (info)) {
					exeProcess.WaitForExit ();
				}
			} catch (Exception) {
			}
		}

		public static void RsvgConvert (params string[] parameters)
		{
			try {
				var info = new ProcessStartInfo (
  				  "bash", "-c 'rsvg-convert " + string.Join (" ", parameters) + "'"
				);
				Console.WriteLine ("rsvg-convert " + string.Join (" ", parameters));
				info.WindowStyle = ProcessWindowStyle.Hidden;
				Console.Write (".");
				using (Process exeProcess = Process.Start (info)) {
					exeProcess.WaitForExit ();
				}
			} catch (Exception) {
			}
		}

		public static void InkScape (params string[] parameters)
		{
			try {
				var info = new ProcessStartInfo (
  				  "bash", "-c 'inkscape " + string.Join (" ", parameters) + "'"
				);
				Console.WriteLine ("inkscape " + string.Join (" ", parameters));
				info.WindowStyle = ProcessWindowStyle.Hidden;
				Console.Write (".");
				using (Process exeProcess = Process.Start (info)) {
					exeProcess.WaitForExit ();
				}
			} catch (Exception) {
			}
		}
	}
}

