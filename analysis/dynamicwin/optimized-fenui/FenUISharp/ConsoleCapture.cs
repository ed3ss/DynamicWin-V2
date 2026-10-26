using System;
using System.IO;
using System.Text;

namespace FenUISharp;

internal static class ConsoleCapture
{
	private class MultiWriter : TextWriter
	{
		private readonly TextWriter[] writers;

		public override Encoding Encoding => System.Text.Encoding.UTF8;

		public MultiWriter(params TextWriter[] writers)
		{
			this.writers = writers;
		}

		public override void Write(char value)
		{
			TextWriter[] array = writers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Write(value);
			}
		}

		public override void Write(string value)
		{
			TextWriter[] array = writers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Write(value);
			}
		}

		public override void WriteLine(string value)
		{
			TextWriter[] array = writers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].WriteLine(value);
			}
		}
	}

	private static StringWriter logWriter = new StringWriter();

	private static TextWriter originalConsoleOut = Console.Out;

	public static void StartCapture()
	{
		string? diagnosticPath = Environment.GetEnvironmentVariable("DYNAMICWIN_LOG");
		TextWriter output = originalConsoleOut;
		if (!string.IsNullOrEmpty(diagnosticPath))
			output = TextWriter.Synchronized(new StreamWriter(diagnosticPath, append: false) { AutoFlush = true });
		Console.SetOut(TextWriter.Synchronized(new MultiWriter(output, logWriter)));
		Console.SetError(Console.Out);
	}

	public static string GetLog()
	{
		return logWriter.ToString();
	}

	public static void SaveErrorLogToFile(string path)
	{
		string contents = $"======== fenUI ERROR LOG: {DateTime.Now.ToLongDateString()}, {DateTime.Now.ToLongTimeString()} ========" + "\r\n\r\n" + logWriter.ToString();
		File.WriteAllText(path, contents);
	}
}
