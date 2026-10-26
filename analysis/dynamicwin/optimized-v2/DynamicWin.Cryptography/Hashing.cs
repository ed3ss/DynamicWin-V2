using System.Security.Cryptography;
using System.Text;

namespace DynamicWin.Cryptography;

public static class Hashing
{
	public static byte[] GetHash(string inputString)
	{
		using HashAlgorithm hashAlgorithm = SHA256.Create();
		return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
	}

	public static string GetHashString(string inputString)
	{
		StringBuilder stringBuilder = new StringBuilder();
		byte[] hash = GetHash(inputString);
		foreach (byte b in hash)
		{
			stringBuilder.Append(b.ToString("X2"));
		}
		return stringBuilder.ToString();
	}
}
