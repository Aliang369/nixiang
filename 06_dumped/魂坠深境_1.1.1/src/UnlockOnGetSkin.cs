using System;
using System.Collections.Generic;

public class UnlockOnGetSkin : GlobalTriggerObserver
{
	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		applicationState.eventManager.unlockCharacterEvent.AddListener(OnGetCharacter);
		void OnGetCharacter(PlayerCharacterDef character)
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
