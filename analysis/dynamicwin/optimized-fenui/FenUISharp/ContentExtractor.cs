using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace FenUISharp;

public static class ContentExtractor
{
	public static string ExtractToFile(string shortOrFullResourceName, string destinationPath, Assembly assembly = null, bool overwrite = false, string expectedSha256 = null)
	{
		if ((object)assembly == null)
		{
			assembly = Assembly.GetCallingAssembly();
		}
		string text = ResolveResourceName(assembly, shortOrFullResourceName);
		if (text == null)
		{
			throw new ArgumentException($"Resource \"{shortOrFullResourceName}\" not found in {assembly.FullName}.");
		}
		if (File.Exists(destinationPath) && !overwrite)
		{
			if (!string.IsNullOrWhiteSpace(expectedSha256))
			{
				EnsureHash(destinationPath, expectedSha256);
			}
			return destinationPath;
		}
		using (Stream stream = assembly.GetManifestResourceStream(text) ?? throw new InvalidOperationException("Failed to open resource stream: " + text))
		{
			using FileStream destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.Read);
			stream.CopyTo(destination);
		}
		if (!string.IsNullOrWhiteSpace(expectedSha256))
		{
			EnsureHash(destinationPath, expectedSha256);
		}
		return destinationPath;
	}

	public static string ExtractToTemp(string shortOrFullResourceName, string fileName = null, Assembly assembly = null, bool overwrite = false, string expectedSha256 = null)
	{
		if (fileName == null)
		{
			fileName = shortOrFullResourceName.Split('.').Last();
		}
		string destinationPath = Path.Combine(Path.GetTempPath(), fileName);
		return ExtractToFile(shortOrFullResourceName, destinationPath, assembly, overwrite, expectedSha256);
	}

	public static byte[] ExtractToMemory(string shortOrFullResourceName, Assembly assembly = null)
	{
		if ((object)assembly == null)
		{
			assembly = Assembly.GetCallingAssembly();
		}
		string text = ResolveResourceName(assembly, shortOrFullResourceName) ?? throw new ArgumentException("Resource \"" + shortOrFullResourceName + "\" not found.");
		using Stream stream = assembly.GetManifestResourceStream(text) ?? throw new InvalidOperationException("Failed to open resource stream: " + text);
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	public static string[] ListAll(Assembly assembly = null)
	{
		if ((object)assembly == null)
		{
			assembly = Assembly.GetCallingAssembly();
		}
		return assembly.GetManifestResourceNames();
	}

	private static string ResolveResourceName(Assembly assembly, string shortOrFull)
	{
		string[] manifestResourceNames = assembly.GetManifestResourceNames();
		string text = manifestResourceNames.FirstOrDefault((string n) => n.Equals(shortOrFull, StringComparison.Ordinal));
		if (text != null)
		{
			return text;
		}
		List<string> list = manifestResourceNames.Where((string n) => n.EndsWith(shortOrFull, StringComparison.OrdinalIgnoreCase)).ToList();
		return (string)((list.Count switch
		{
			0 => null, 
			1 => list[0], 
			_ => throw new ArgumentException("Resource name \"" + shortOrFull + "\" is ambiguous. Candidates:\n" + string.Join("\n", list)), 
		}) ?? "");
	}

	private static void EnsureHash(string path, string expectedSha256)
	{
		string text = ComputeSha256(path);
		if (!text.Equals(NormalizeHash(expectedSha256), StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException($"SHA-256 mismatch for {path}. Expected {expectedSha256}, got {text}.");
		}
	}

	private static string ComputeSha256(string path)
	{
		using SHA256 sHA = SHA256.Create();
		using FileStream inputStream = File.OpenRead(path);
		return BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace("-", "").ToLowerInvariant();
	}

	private static string NormalizeHash(string h)
	{
		return h.Replace(" ", "").Replace("-", "").ToLowerInvariant();
	}
}
