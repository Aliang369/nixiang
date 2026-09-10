using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class ToCameraFOV : CameraControlNode
{
	public float targetValue;

	public float time;

	public AnimationCurve curve;

	public override Sequence ExecuteCor(ApplicationStateInfo applicationState)
	{
		FinalRenderCamera cam = GetVirtualCamera(applicationState).bindedCamera;
		Sequence sequence = DOTween.Sequence();
		TweenerCore<float, float, FloatOptions> t = DOTween.To(() => cam.CameraFOV, delegate(float v)
		{
			cam.CameraFOV = v;
		}, targetValue, time);
		if (curve != null)
		{
			t.SetEase(curve);
		}
		sequence.Append(t);
		return sequence;
	}
}
