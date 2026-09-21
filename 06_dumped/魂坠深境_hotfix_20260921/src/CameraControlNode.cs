using DG.Tweening;

public abstract class CameraControlNode
{
	public abstract Sequence ExecuteCor(ApplicationStateInfo applicationState);

	public Sequence Run(ApplicationStateInfo applicationState, float battleScale)
	{
		VirtualCamera virtualCamera = GetVirtualCamera(applicationState);
		if (virtualCamera.currentSequence != null && !virtualCamera.currentSequence.IsComplete())
		{
			virtualCamera.currentSequence.Complete(withCallbacks: true);
			virtualCamera.currentSequence = null;
		}
		virtualCamera.currentSequence = ExecuteCor(applicationState).Play();
		virtualCamera.currentSequence.timeScale = battleScale;
		virtualCamera.currentSequence.Play();
		return virtualCamera.currentSequence;
	}

	public VirtualCamera GetVirtualCamera(ApplicationStateInfo applicationState)
	{
		if (applicationState.runningGameState != null)
		{
			return applicationState.runningGameState.UIManager.gameVirtualCamera;
		}
		if (applicationState.fortressState != null)
		{
			return applicationState.fortressState.uiManager.fortressCamera;
		}
		return null;
	}
}
