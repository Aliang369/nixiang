public class EnterBattleEvent : WorldEvent
{
	public override WorldEventDisplayType displayType => WorldEventDisplayType.Battle;

	public override WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return null;
	}

	protected override void Notify()
	{
		base.Notify();
		if (gameState.applicationState.settingManager.uiTimeScaleTypeSetting == TimeScaleManager.TimeScaleType.X1)
		{
			EnqueueAction(new WaitAction(0.5f));
			EnqueueAction(new NotifyBannerHintAction(new BannerHintDisplay.SetupInfo
			{
				hintText = ScriptableObjectSingleton<TextDataPreset>.instance.term_encounter_arg0.FormatWith(GetEnemyStr()),
				soundType = BannerHintDisplay.SoundType.EnemyTurnStart,
				backgroundType = BannerHintDisplay.BackgroundType.BattleStart,
				textType = BannerHintDisplay.TextType.Default,
				tweenTime = ScriptableObjectSingleton<VFXDataPreset>.instance.playerBftUIDisplayConfig.bannerTweenTime,
				stayTime = ScriptableObjectSingleton<VFXDataPreset>.instance.playerBftUIDisplayConfig.bannerStayTime
			}));
		}
		EnqueueAction(new EnterBattleAction());
		SetupEnvironmentInfluence();
		EnqueueAction(new FinishedWorldEventAction(this, exitRoom: false));
	}

	private void SetupEnvironmentInfluence()
	{
		if (!gameState.IsIntroGame && gameState.dungeonMapManager.currentLocation?.locationEnvironmentInfluence != null)
		{
			EnqueueAction(new SetEnvironmentInfluenceAction(gameState.dungeonMapManager.currentLocation.locationEnvironmentInfluence, hint: true));
		}
	}

	protected override void UpdateDisplayInfo(WorldEventDisplayInfo displayInfo)
	{
		displayInfo.title = string.Format(EventName, GetEnemyStr());
		displayInfo.description = string.Format(EventDescription, GetEnemyStr());
	}

	private string GetEnemyStr()
	{
		return gameState.currentEncounter.enemyAI?.EnemyName;
	}
}
