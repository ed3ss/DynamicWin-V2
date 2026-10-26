using System;

namespace FenUISharp;

internal class Program
{
	[STAThread]
	private static void Main()
	{
		FenUI.Init();
		FenUI.SetupAppModel("FlorianButz.fenUI");
		FenUI.EnableDebugFunctions();
		FenUI.Demo();
	}
}
