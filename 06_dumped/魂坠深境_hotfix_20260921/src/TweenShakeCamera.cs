using DG.Tweening;
using UnityEngine;

public class TweenShakeCamera : CameraControlNode
{
	public float duration;

	public Vector3 shakePositionStrength;

	public int positionVibrato;

	public float positionRandomness;

	public ShakeRandomnessMode positionRandomnessMode;

	public Vector3 shakeRotationStrength;

	public int rotationVibrato;

	public float rotationRandomness;

	public ShakeRandomnessMode rotationRandomnessMode;

	public override Sequence ExecuteCor(ApplicationStateInfo applicationState)
	{
		Sequence sequence = DOTween.Sequence();
		VirtualCamera virtualCamera = GetVirtualCamera(applicationState);
		Tweener t = virtualCamera.shakeEffectRoot.DOShakePosition(duration, shakePositionStrength, positionVibrato, positionRandomness, snapping: false, fadeOut: true, positionRandomnessMode);
		Tweener t2 = virtualCamera.shakeEffectRoot.DOShakeRotation(duration, shakeRotationStrength, rotationVibrato, rotationRandomness, fadeOut: true, rotationRandomnessMode);
		sequence.Join(t);
		sequence.Join(t2);
		return sequence;
	}
}
