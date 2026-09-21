using DG.Tweening;
using UnityEngine;

public class AnimationControl : CameraControlNode
{
	public AnimationClip clip;

	public override Sequence ExecuteCor(ApplicationStateInfo applicationState)
	{
		VirtualCamera cam = GetVirtualCamera(applicationState);
		cam.moveEffectAnimation.AddClip(clip, ((Object)(object)clip).name);
		cam.moveEffectAnimation.Play(((Object)(object)clip).name);
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(clip.length);
		sequence.AppendCallback(delegate
		{
			cam.moveEffectAnimation.Stop();
		});
		return sequence;
	}
}
