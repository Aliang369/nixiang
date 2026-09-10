using System;
using System.Collections.Generic;

public class OnUnlockMasteryTalentGuide : GlobalTriggerObserver
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
		applicationState.eventManager.activityMissionTalentUnlockedEvent.AddListener(OnUnlockTalent);
		applicationState.eventManager.characterTalentPanelClosedEvent.AddListener(OnTalentPanelClosed);
		void OnTalentPanelClosed(PlayerCharacterState character)
		{
			StoryPointManager storyPointManager = applicationState.storyPointManager;
			if (character != null && storyPointManager.TryMarkMasteryTalentGuideTriggered(character))
			{
				onTrigger();
			}
		}
		void OnUnlockTalent(YuanQiActivityMissionTalentEventData data)
		{
			if (data?.character != null && !(data.talent == null) && data.talent.talentType == TalentDef.TalentType.MasteryUnlock)
			{
				applicationState.storyPointManager.TryCaptureMasteryTalentGuideContext(data.character, data.talent);
			}
		}
	}
}
