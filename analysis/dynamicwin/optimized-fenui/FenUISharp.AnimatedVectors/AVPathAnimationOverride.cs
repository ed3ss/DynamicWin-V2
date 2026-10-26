using FenUISharp.Mathematics;

namespace FenUISharp.AnimatedVectors;

public class AVPathAnimationOverride
{
	public Vector2 Anchor = new Vector2(0.5f, 0.5f);

	public Vector2 Translation = new Vector2(0f, 0f);

	public Vector2 Scale = new Vector2(1f, 1f);

	public float Rotation;

	public float Opacity = 1f;

	public float BlurRadius;

	public float StrokeTrace = 1f;

	public bool UseObjectAnchor { get; set; }

	public bool UseObjectSizeTranslation { get; set; }
}
