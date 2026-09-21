using DG.Tweening;

public class ControlCameraAction : GameAction
{
	private CameraControlData data;

	private bool delay;

	private Sequence seq;

	public ControlCameraAction(CameraControlData data, bool delay = false)
	{
		this.data = data;
		this.delay = delay;
	}

	protected override void Update()
	{
		if (seq == null)
		{
			seq = data.Run(gameState.applicationState, base.actionTimeScale);
		}
		if (delay && !gameState.actionManager.skipFrameAnim)
		{
			if (seq.IsComplete() || !seq.IsActive())
			{
				FinishAction();
			}
		}
		else
		{
			FinishAction();
		}
	}
}
