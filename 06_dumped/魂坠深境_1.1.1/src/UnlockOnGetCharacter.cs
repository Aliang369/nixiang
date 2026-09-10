using System;
using System.Collections.Generic;

public class UnlockOnGetCharacter : GlobalTriggerObserver
{
	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		applicationState.eventManager.unlockCharacterClassEvent.AddListener(OnGetCharacter);
		void OnGetCharacter(PlayerCharacterDef info)
		{
			onTrigger();
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return null;
	}
}
