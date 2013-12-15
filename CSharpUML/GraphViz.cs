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
  				  "dot", "-T" + format + " -o'" + output + "' '" + input + "'" + " -Gdpi=300"
				);
				Process.Start (info);
			} catch (Exception ex) {
			}
		}
	}
}

