namespace FenUISharp.Objects.Text.Model;

public class TextSpan
{
	public string Content { get; init; }

	public TextStyle Style { get; init; }

	public float CharacterSpacing { get; init; }

	public TextSpan(string content, TextStyle style)
	{
		Content = content;
		Style = style;
	}
}
