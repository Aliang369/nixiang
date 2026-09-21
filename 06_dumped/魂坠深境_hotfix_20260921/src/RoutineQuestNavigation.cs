using System;
using System.Linq;
using ContainerOfSoul.Display.Fortress;

public static class RoutineQuestNavigation
{
	public static bool TryJumpToPanel(RoutineQuest quest, ApplicationStateInfo applicationState, Action beforeJump = null)
	{
		FortressUIManager fortressUIManager = (applicationState?.fortressState)?.uiManager;
		if (quest == null || fortressUIManager == null)
		{
			return false;
		}
		switch (quest.JumpPanelType)
		{
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.None:
			return false;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.StartGame:
			beforeJump?.Invoke();
			fortressUIManager.ShowUI(fortressUIManager.baseGameUIPanel);
			return true;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.SkillUpgrade:
			beforeJump?.Invoke();
			fortressUIManager.ShowUI(fortressUIManager.skill_GemPanel);
			return true;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.StarMap:
			beforeJump?.Invoke();
			fortressUIManager.mainUIPanel.mainUISwitchGroup.mainPanelLayerSetups.FirstOrDefault((FortressMainUIPanel.MainPanelLayerSetup layer) => layer.layer == FortressMainUIPanel.MainPanelLayer.StargazingRoom)?.button.onClick?.Invoke();
			return true;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.GachaDraw:
			beforeJump?.Invoke();
			fortressUIManager.gachaPanel.ShowDefaultEntryPool();
			return true;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.CombineGem:
			beforeJump?.Invoke();
			fortressUIManager.skill_GemPanel.gemCombinePanel.ShowUI();
			return true;
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.CharacterInfo:
		{
			PlayerCharacterState playerCharacterState = applicationState.newGameConfigManager.currentNewGameConfig?.GetCharacterState(applicationState) ?? applicationState.characterManager.currentCharacter;
			if (playerCharacterState == null)
			{
				return false;
			}
			beforeJump?.Invoke();
			fortressUIManager.characterInfoBasePanel.SetupFromMainRoom(playerCharacterState);
			return true;
		}
		case RoutineQuestDef.RoutineQuestNode.JumpPanelType.SkillGachaDraw:
			beforeJump?.Invoke();
			fortressUIManager.gachaPanel.ShowEntryPool(GachaPoolDef.GachaPoolType.SkillStable);
			return true;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
