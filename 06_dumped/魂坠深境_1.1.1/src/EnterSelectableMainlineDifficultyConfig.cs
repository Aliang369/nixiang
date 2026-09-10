using System;
using System.Collections.Generic;

public class EnterSelectableMainlineDifficultyConfig : GlobalTriggerObserver
{
	public DifficultyLevel difficultyLevel;

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return null;
	}

	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		applicationState.eventManager.enterFortressGameConfigPanel.AddListener(OnEnterGameConfig);
		void OnEnterGameConfig(NewGameConfigData config)
		{
			if (config != null && config.contract?.contractDef?.contractType == ContractDef.ContractType.BaseGame)
			{
				DailyChallengeRunContext dailyChallengeContext = config.dailyChallengeContext;
				if ((dailyChallengeContext == null || !dailyChallengeContext.isDailyChallenge) && config.characterUsageMode != CharacterUsageMode.LimitedDeployment && config.CanSelectDifficulty(applicationState, difficultyLevel))
				{
					onTrigger();
				}
			}
		}
	}
}
