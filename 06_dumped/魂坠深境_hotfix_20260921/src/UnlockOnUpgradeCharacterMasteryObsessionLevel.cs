using System;
using System.Collections.Generic;

public class UnlockOnUpgradeCharacterMasteryObsessionLevel : GlobalTriggerObserver
{
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
		applicationState.eventManager.upgradeCharacterObsessionLevelEvent.AddListener(delegate((PlayerCharacterState, CharacterObsessionLevelConfig) tuple)
		{
			if (tuple.Item2.effect is UnlockCharacterMastery || tuple.Item2.effect is UnlockAllMastery)
			{
				onTrigger();
			}
		});
	}
}
