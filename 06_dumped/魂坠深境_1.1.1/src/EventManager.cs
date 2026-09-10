using System.Collections.Generic;
using ContainerOfSoul.Display.Fortress;
using UnityEngine;

public class EventManager : GlobalGameManager
{
	public class LineLevelTriggerEventData
	{
		public LevelLine levelLine;

		public LevelLineDef.LineLevel newLevel;

		public LineLevelTriggerEventData(LevelLine levelLine, LevelLineDef.LineLevel newLevel)
		{
			this.levelLine = levelLine;
			this.newLevel = newLevel;
		}
	}

	public class CharacterLevelupEventData
	{
		public CharacterClassState characterClass;

		public LevelLineDef.LineLevel newLevel;

		public LevelLineDef.LineLevel oldLevel;

		public CharacterLevelupEventData(CharacterClassState characterClass, LevelLineDef.LineLevel oldLevel, LevelLineDef.LineLevel newLevel)
		{
			this.characterClass = characterClass;
			this.oldLevel = oldLevel;
			this.newLevel = newLevel;
		}
	}

	public EventInfo<Achievement> achievementAchieveEvent = new EventInfo<Achievement>();

	public EventInfo<(ProfileSkill, SkillWordDef)> addSkillWordEvent = new EventInfo<(ProfileSkill, SkillWordDef)>();

	public EventInfo<CharacterClassState> characterClassExpChangeEvent = new EventInfo<CharacterClassState>();

	public EventInfo<CharacterLevelupEventData> characterLevelUpEvent = new EventInfo<CharacterLevelupEventData>();

	public EventInfo<ContractState> ContractRefreshedEvent = new EventInfo<ContractState>();

	public EventInfo<ProfileSkill> deleteSkillEvent = new EventInfo<ProfileSkill>();

	public EventInfo<ProfileSkill> drawSkillEvent = new EventInfo<ProfileSkill>();

	public EventInfoNoData enterWorldEventEvent = new EventInfoNoData();

	public EventInfo<Contract> finishContractEvent = new EventInfo<Contract>();

	public EventInfoNoData fortressInitFinishEvent = new EventInfoNoData();

	public EventInfoNoData enterFortressPanelEvent = new EventInfoNoData();

	public EventInfo<FortressMainUIPanel.MainPanelLayer> fortressMainPanelLayerChangedEvent = new EventInfo<FortressMainUIPanel.MainPanelLayer>();

	public EventInfoNoData returnToFortressMainRoomEvent = new EventInfoNoData();

	public EventInfo<PlayerCharacterState> characterTalentPanelClosedEvent = new EventInfo<PlayerCharacterState>();

	public EventInfo<NewGameConfigData> enterFortressGameConfigPanel = new EventInfo<NewGameConfigData>();

	public EventInfo<(PresetSkillSlot slot, ProfileSkill skill)> configSkillPresetEvent = new EventInfo<(PresetSkillSlot, ProfileSkill)>();

	public EventInfoNoData gameEnterEvent = new EventInfoNoData();

	public EventInfoWithID<GameEvent, GameEvent.Type> gameEvent = new EventInfoWithID<GameEvent, GameEvent.Type>();

	public EventInfo<YuanQiActivityMissionGiftCodeEventData> activityMissionGiftCodeEvent = new EventInfo<YuanQiActivityMissionGiftCodeEventData>();

	public EventInfo<YuanQiActivityMissionShareEventData> activityMissionShareEvent = new EventInfo<YuanQiActivityMissionShareEventData>();

	public EventInfo<YuanQiActivityMissionTalentEventData> activityMissionTalentUnlockedEvent = new EventInfo<YuanQiActivityMissionTalentEventData>();

	public EventInfo<MatterInfo> matterChangeEvent = new EventInfo<MatterInfo>();

	public EventInfo<List<MailData>> mailStateUpdateEvent = new EventInfo<List<MailData>>();

	public EventInfo<StoryPoint> storyPointFinished = new EventInfo<StoryPoint>();

	public EventInfo<ICTAsset> objectDiscoveredEvent = new EventInfo<ICTAsset>();

	public EventInfo<StoryEventObserver.Type> storyEvent = new EventInfo<StoryEventObserver.Type>();

	public EventInfoNoData OpenSelectSkillPanelEvent = new EventInfoNoData();

	public EventInfo<int> purchasePayEvent = new EventInfo<int>();

	public EventInfo<LevelLine.ChangeExpResult> changeExpResultExecuteEvent = new EventInfo<LevelLine.ChangeExpResult>();

	public EventInfo<GlobalRewardInfo> ReceiveRewardEvent = new EventInfo<GlobalRewardInfo>();

	public EventInfo<(ProfileSkill, SkillWordDef)> removeSkillWordEvent = new EventInfo<(ProfileSkill, SkillWordDef)>();

	public EventInfoNoData RoutineQuestRefreshedEvent = new EventInfoNoData();

	public EventInfo<NewGameConfigData> SetNewGameConfigEvent = new EventInfo<NewGameConfigData>();

	public EventInfo<SelectGetItemBonus> showItemBonusSelectionPanelEvent = new EventInfo<SelectGetItemBonus>();

	public EventInfo<CharacterClassState> strengthCharacterEvent = new EventInfo<CharacterClassState>();

	public EventInfoNoData UIDungeonMapShowedEvent = new EventInfoNoData();

	public EventInfoNoData UIEmptyClickedEvent = new EventInfoNoData();

	public EventInfo<Transform> UIHideAllInfoEvent = new EventInfo<Transform>();

	public EventInfoNoData UIHideDungeonMapEvent = new EventInfoNoData();

	public EventInfo<PlayerCharacterDef> unlockCharacterClassEvent = new EventInfo<PlayerCharacterDef>();

	public EventInfo<PlayerCharacterDef> unlockCharacterEvent = new EventInfo<PlayerCharacterDef>();

	public EventInfo<ItemSkinDef> unlockItemSkinEvent = new EventInfo<ItemSkinDef>();

	public EventInfo<LineLevelTriggerEventData> LineLevelTriggerEvent = new EventInfo<LineLevelTriggerEventData>();

	public EventInfo<TalentDef> unlockTalentEvent = new EventInfo<TalentDef>();

	public EventInfo<ClavisDef> UnlockClavisEvent = new EventInfo<ClavisDef>();

	public EventInfo<ClavisDef> ActivateClavisEvent = new EventInfo<ClavisDef>();

	public EventInfo<FortressNPC> talkToFortressNPCEvent = new EventInfo<FortressNPC>();

	public EventInfo<FinishBattleEncounterAction.FinishType> IntroFinalBossBattleEnd = new EventInfo<FinishBattleEncounterAction.FinishType>();

	public EventInfo<DailyCheckInResult> dailyCheckInEvent = new EventInfo<DailyCheckInResult>();

	public EventInfo<GameRecord> afterGameOverEvent = new EventInfo<GameRecord>();

	public EventInfo<IdolState> idolUpgradeCompleteEvent = new EventInfo<IdolState>();

	public EventInfo<ProfileSkill> addSkillGemSlotEvent = new EventInfo<ProfileSkill>();

	public EventInfo<ProfileSkill> breakThroughSkillEvent = new EventInfo<ProfileSkill>();

	public EventInfo<SkillGem> combineGemEvent = new EventInfo<SkillGem>();

	public EventInfo<(GachaPoolDef, int)> gachaDrawEvent = new EventInfo<(GachaPoolDef, int)>();

	public EventInfo<(PlayerCharacterState, CharacterObsessionLevelConfig)> upgradeCharacterObsessionLevelEvent = new EventInfo<(PlayerCharacterState, CharacterObsessionLevelConfig)>();

	public EventInfo<SavingUtil.GlobalConfigSaveData> GlobalConfigDataChanged = new EventInfo<SavingUtil.GlobalConfigSaveData>();

	public EventInfoNoData speedChanged = new EventInfoNoData();

	public EventInfo<bool> shareCompletedEvent = new EventInfo<bool>();

	public override void Init(ApplicationStateInfo applicationState)
	{
		applicationState.eventManager.GlobalConfigDataChanged.AddListener(Bootstrap.instance.audioManager.OnGlobalConfigChanged);
	}

	public override void Dispose()
	{
		base.applicationState.eventManager.GlobalConfigDataChanged.RemoveListener(Bootstrap.instance.audioManager.OnGlobalConfigChanged);
	}
}
