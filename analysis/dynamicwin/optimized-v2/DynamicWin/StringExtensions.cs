namespace DynamicWin;

public static class StringExtensions
{
	public static string Truncate(this string value, int maxChars)
	{
		if (value.Length > maxChars)
		{
			return value.Substring(0, maxChars) + "…";
		}
		return value;
	}
}
