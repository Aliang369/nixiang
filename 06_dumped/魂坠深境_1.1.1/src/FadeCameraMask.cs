using DG.Tweening;
using ProceduralToolkit;
using UnityEngine;
using UnityEngine.UI;

public class FadeCameraMask : CameraControlNode
{
	public Sprite sprite;

	public AnimationCurve curve;

	public override Sequence ExecuteCor(ApplicationStateInfo applicationState)
	{
		VirtualCamera cam = GetVirtualCamera(applicationState);
		if (cam.cameraEffectImagePool == null)
		{
			return DOTween.Sequence();
		}
		Image image = cam.cameraEffectImagePool.Spawn();
		image.gameObject.SetActive(value: true);
		image.sprite = sprite;
		image.color = Color.white.WithA(0f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(DOTweenModuleUI.DOFade(image, 1f, curve[curve.length - 1].time));
		if (curve != null)
		{
			sequence.SetEase(curve);
		}
		sequence.AppendCallback(delegate
		{
			cam.cameraEffectImagePool.Despawn(image);
		});
		sequence.Play();
		return DOTween.Sequence();
	}
}
