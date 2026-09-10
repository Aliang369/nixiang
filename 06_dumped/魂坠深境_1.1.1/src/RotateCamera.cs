using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class RotateCamera : CameraControlNode
{
	public Vector3 target;

	public float time;

	public AnimationCurve curve;

	public override Sequence ExecuteCor(ApplicationStateInfo applicationState)
	{
		TweenerCore<Quaternion, Vector3, QuaternionOptions> t = GetVirtualCamera(applicationState).MoveEffectRoot.DOLocalRotate(target, time);
		if (curve != null)
		{
			t.SetEase(curve);
		}
		Sequence sequence = DOTween.Sequence();
		sequence.Append(t);
		return sequence;
	}
}
