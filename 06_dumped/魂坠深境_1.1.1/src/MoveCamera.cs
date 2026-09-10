using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class MoveCamera : CameraControlNode
{
	public Vector3 target;

	public float time;

	public AnimationCurve curve;

	public override Sequence ExecuteCor(ApplicationStateInfo applicationState)
	{
		TweenerCore<Vector3, Vector3, VectorOptions> t = GetVirtualCamera(applicationState).MoveEffectRoot.DOLocalMove(target, time);
		if (curve != null)
		{
			t.SetEase(curve);
		}
		Sequence sequence = DOTween.Sequence();
		sequence.Append(t);
		return sequence;
	}
}
