using System;
using System.Diagnostics;

namespace CSharpUML
{
	public static class GraphViz
	{
		public static void Dot (string format, string input, string output)
		{
			var info = new ProcessStartInfo (
  				  "dot", "-T" + format + " -o'" + output + "' '" + input + "'"
			);
			Process.Start (info);
		}
	}
}

