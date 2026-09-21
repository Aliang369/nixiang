using System;
using UnityEngine;

[Serializable]
public sealed class HoldRepeatCastConfig
{
	private const float SigmoidHalfSpan = 3f;

	public float holdDuration = 2f;

	public float initialInterval = 0.6f;

	public float minimumInterval = 0.2f;

	public int accelerationCastCount = 15;

	public float GetIntervalAfterCast(int continuousCastIndex)
	{
		float num = Mathf.Max(0.01f, initialInterval);
		float b = Mathf.Clamp(minimumInterval, 0.01f, num);
		int num2 = Mathf.Max(1, accelerationCastCount);
		float num3 = Mathf.Clamp01((float)continuousCastIndex / (float)num2);
		float value = 1f / (1f + Mathf.Exp(-6f * (num3 - 0.5f)));
		float a = 1f / (1f + Mathf.Exp(3f));
		float b2 = 1f / (1f + Mathf.Exp(-3f));
		float t = Mathf.InverseLerp(a, b2, value);
		return Mathf.Lerp(num, b, t);
	}
}
