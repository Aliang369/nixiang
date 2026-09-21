using ChillyRoom.Services.ContainerOfSoul;

public abstract class GuideModule
{
	protected ApplicationStateInfo applicationState;

	public string id => GetType().Name;

	public bool IsValidAsset()
	{
		return true;
	}

	protected void EnqueueAction(GameAction acion)
	{
		applicationState.runningGameState.actionManager.EnqueueAction(acion);
	}

	public virtual void Init(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState;
	}

	public virtual void RunModule()
	{
	}

	public void Finish()
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			new GeneralGameAsyncOperationRequestOnly<FinishGuideModuleRequest>(new FinishGuideModuleRequest(this), applicationState.ChillyRoomSDK.Guide.FinishGuideModuleAsync).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_generating_guide_progress);
		}
		if (!applicationState.guideManager.finishedGuides.Contains(id))
		{
			applicationState.guideManager.finishedGuides.Add(id);
		}
	}
}
