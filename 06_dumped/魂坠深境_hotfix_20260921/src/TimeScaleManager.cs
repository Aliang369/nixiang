public class TimeScaleManager : GlobalGameManager
{
	public enum TimeScaleType
	{
		X1,
		X2,
		X4
	}

	public float battleTimeScaleSetting;

	public float uiTimeScaleSetting;

	public SavingUtil.GlobalConfigSaveData GlobalConfigData => Bootstrap.instance.globalSaveConfigData;

	public TimeScaleType battleTimeScaleTypeSetting => GlobalConfigData.battleTimeScaleTypeSetting;

	public TimeScaleType uiTimeScaleTypeSetting => GlobalConfigData.uiTimeScaleTypeSetting;

	public float GetBattleScale(float actionScale = 1f)
	{
		return base.applicationState.runningGameState?.actionManager.GetBattleScale(actionScale) ?? 1f;
	}

	public float GetUIScale(float actionScale = 1f)
	{
		return base.applicationState.runningGameState?.actionManager.GetUIScale(actionScale) ?? 1f;
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		_OnGlobalConfigChanged(GlobalConfigData);
		applicationState.eventManager.GlobalConfigDataChanged.AddListener(_OnGlobalConfigChanged);
	}

	public override void Dispose()
	{
		base.applicationState.eventManager.GlobalConfigDataChanged.RemoveListener(_OnGlobalConfigChanged);
	}

	private void _OnGlobalConfigChanged(SavingUtil.GlobalConfigSaveData globalConfigData)
	{
		battleTimeScaleSetting = battleTimeScaleTypeSetting switch
		{
			TimeScaleType.X2 => 0.5f, 
			TimeScaleType.X4 => 1f, 
			_ => 0f, 
		};
		uiTimeScaleSetting = uiTimeScaleTypeSetting switch
		{
			TimeScaleType.X2 => 0.5f, 
			TimeScaleType.X4 => 1f, 
			_ => 0f, 
		};
	}
}
