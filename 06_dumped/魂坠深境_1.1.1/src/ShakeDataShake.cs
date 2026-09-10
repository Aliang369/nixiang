using DG.Tweening;
using FirstGearGames.SmoothCameraShaker;

public class ShakeDataShake : CameraControlNode
{
	public ShakeData data;

	public override Sequence ExecuteCor(ApplicationStateInfo applicationState)
	{
		VirtualCamera virtualCamera = GetVirtualCamera(applicationState);
		ShakerInstance si = virtualCamera.cameraShaker.Shake(data);
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(data.TotalDuration);
		sequence.AppendCallback(delegate
		{
			si.Stop();
		});
		return sequence;
	}
}
