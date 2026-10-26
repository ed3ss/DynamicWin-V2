using System;
using System.Collections.Generic;

namespace FenUISharp.AnimatedVectors;

public class AVAnimation
{
	public int[] AffectedPathIDs { get; init; } = new int[0];

	public bool UseObjectAnchor { get; init; }

	public bool UseObjectSizeTranslation { get; init; }

	public bool DontResetValues { get; init; }

	public bool PerKeyframeEase { get; init; }

	public float Duration { get; init; } = 1f;

	public float ExtendDuration { get; init; }

	public Func<Func<float, float>> CreateEasing { get; init; } = () => (float x) => x;

	public List<AVKeyframe> Keyframes { get; init; } = new List<AVKeyframe>();
}
