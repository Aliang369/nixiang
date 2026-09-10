using DG.Tweening;

public class WaitCameraControl : CameraControlNode
{
	public float time;

	public override Sequence ExecuteCor(ApplicationStateInfo applicationState)
	{
		return DOTween.Sequence().AppendInterval(time);
	}
}
