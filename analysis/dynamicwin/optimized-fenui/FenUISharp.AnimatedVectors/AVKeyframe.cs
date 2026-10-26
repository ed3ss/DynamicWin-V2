using System.Collections.Generic;

namespace FenUISharp.AnimatedVectors;

public class AVKeyframe
{
	public float time;

	public List<(string id, object value)> attributes;
}
