using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class StringStream
{
	public static string Write(string input, string p)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(input);
		using Aes aes = Aes.Create();
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(p, new byte[13]
		{
			73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
			100, 101, 118
		});
		aes.Key = rfc2898DeriveBytes.GetBytes(32);
		aes.IV = rfc2898DeriveBytes.GetBytes(16);
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
		{
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.Close();
		}
		input = Convert.ToBase64String(memoryStream.ToArray());
		return input;
	}

	public static string Read(string input, string p)
	{
		input = input.Replace(" ", "+");
		byte[] array = Convert.FromBase64String(input);
		using Aes aes = Aes.Create();
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(p, new byte[13]
		{
			73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
			100, 101, 118
		});
		aes.Key = rfc2898DeriveBytes.GetBytes(32);
		aes.IV = rfc2898DeriveBytes.GetBytes(16);
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
		{
			cryptoStream.Write(array, 0, array.Length);
			cryptoStream.Close();
		}
		input = Encoding.Unicode.GetString(memoryStream.ToArray());
		return input;
	}
}
