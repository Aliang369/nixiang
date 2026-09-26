using Proto.Enums;
using Proto.Types;

public class zbSelf
{
	private static zbSelf sInstance;

	public zbTimeHandler timeHandler;

	public zbDataHandler dataHandler;

	public zbBarHandler barHandler;

	public zbChatHandler chatHandler;

	public zbFriendHandler friendHandler;

	public zbGetWayHandler getWayHandler;

	public zbHeroHandler heroHandler;

	public zbEquipHandler equipHandler;

	public zbItemHandler itemHandler;

	public zbMailHandler mailHandler;

	public zbMoneyHandler moneyHandler;

	public zbNewTagHandler newTagHandler;

	public zbRedPointHandler redPointHandler;

	public zbShopHandler shopHandler;

	public zbTaskHandler taskHandler;

	public zbLevelUpHandler levelUpHandler;

	public zbHomelandHandler homelandHandler;

	public zbHomeEditHandler homeEditHandler;

	public zbFunHandler funHandler;

	public zbLegionHandler legionHandler;

	public zbLegionWarHandler legionWarHandler;

	public zbVipHandler vipHandler;

	public zbMainQuestHandler mainQuestHandler;

	public zbEliteQuestHandler eliteQuestHandler;

	public zbEndlessHordeHandler endlessHordeHandler;

	public zbActivityHandler activityHandler;

	public zbOperateHandler operateHandler;

	public zbHolidayHandler holidayHandler;

	public zbRestaurantHandler restaurantHandler;

	public zbEndlessMineHandler endlessMineHandler;

	public zbSnowVeilHandler snowVeilHandler;

	public zbDungeonHandler worldDungeonHandler;

	public zbWorldHandler worldHandler;

	public zbWorldLairHandler worldLairHandler;

	public zbRankHandler rankHandler;

	public zbWorldBossHandler worldBossHandler;

	public zbHeadHandler headHandler;

	public zbTrialHandler trialHandler;

	public zbGuideHandler guideHandler;

	public zbHandbookHandler handbookHandler;

	public zbCombatHandler combatHandler;

	public zbSuitGunHandler suitGunHandler;

	public zbMonsterHandler monsterHandler;

	public zbPlayerSuggestHandler playerSuggestHandler;

	public zbFuncUnLockHandler funcUnLockHandler;

	public zbPassPortHandler passportHandler;

	public zbElementTrialHandler elementTrialHandler;

	public zbAttrHandler attrHandler;

	public zbMechaHandler mechaHandler;

	public zbJourneyOneHandler journeyOneHandler;

	public zbJourneyTwoHandler journeyTwoHandler;

	public zbJourneyThreeHandler journeyThreeHandler;

	public zbWallHandler wallHandler;

	public zbLegendaryHandler legendaryHandler;

	public zbArenaHandler arenaHandler;

	public zbPvpHandler pvpHandler;

	public zbFootholdHandler guardHandler;

	public zbCrossLadderHandler crossLadderHandler;

	public zbGeneHandler geneHandler;

	public zbTreasureHandler treasureHandler;

	public zbMineWarHandler mineWarHandler;

	public zbGrowthGiftHandler growthGiftHandler;

	public zbServantHandler servantHandler;

	public zbCrossPeakHandler crossPeakHandler;

	public zbReminderHandler reminderHandler;

	public zbTruckHandler truckHandler;

	public zbFacilityHandler facilityHandler;

	public zbBossSubjectHandler bossSubjectHandler;

	public zbWallSubjectHandler wallSubjectHandler;

	public zbMechaSubjectHandler mechaSubjectHandler;

	public zbNotifySetHandler notifySetHandler;

	public zbSeasonHandler seasonHandler;

	public zbTacticHandler tacticHandler;

	private bool m_bLoading = true;

	public static zbSelf Instance => sInstance;

	public static void CreateInstatnce()
	{
		sInstance = new zbSelf();
	}

	public void Init()
	{
		m_bLoading = true;
		timeHandler = new zbTimeHandler();
		dataHandler = new zbDataHandler();
		barHandler = new zbBarHandler();
		chatHandler = new zbChatHandler();
		friendHandler = new zbFriendHandler();
		getWayHandler = new zbGetWayHandler();
		heroHandler = new zbHeroHandler();
		equipHandler = new zbEquipHandler();
		itemHandler = new zbItemHandler();
		mailHandler = new zbMailHandler();
		moneyHandler = new zbMoneyHandler();
		newTagHandler = new zbNewTagHandler();
		redPointHandler = new zbRedPointHandler();
		shopHandler = new zbShopHandler();
		taskHandler = new zbTaskHandler();
		levelUpHandler = new zbLevelUpHandler();
		homelandHandler = new zbHomelandHandler();
		homeEditHandler = new zbHomeEditHandler();
		funHandler = new zbFunHandler();
		legionHandler = new zbLegionHandler();
		legionWarHandler = new zbLegionWarHandler();
		vipHandler = new zbVipHandler();
		mainQuestHandler = new zbMainQuestHandler();
		eliteQuestHandler = new zbEliteQuestHandler();
		endlessHordeHandler = new zbEndlessHordeHandler();
		activityHandler = new zbActivityHandler();
		operateHandler = new zbOperateHandler();
		holidayHandler = new zbHolidayHandler();
		restaurantHandler = new zbRestaurantHandler();
		endlessMineHandler = new zbEndlessMineHandler();
		snowVeilHandler = new zbSnowVeilHandler();
		worldDungeonHandler = new zbDungeonHandler();
		worldHandler = new zbWorldHandler();
		worldLairHandler = new zbWorldLairHandler();
		rankHandler = new zbRankHandler();
		worldBossHandler = new zbWorldBossHandler();
		headHandler = new zbHeadHandler();
		trialHandler = new zbTrialHandler();
		guideHandler = new zbGuideHandler();
		handbookHandler = new zbHandbookHandler();
		combatHandler = new zbCombatHandler();
		suitGunHandler = new zbSuitGunHandler();
		monsterHandler = new zbMonsterHandler();
		playerSuggestHandler = new zbPlayerSuggestHandler();
		funcUnLockHandler = new zbFuncUnLockHandler();
		journeyOneHandler = new zbJourneyOneHandler();
		journeyTwoHandler = new zbJourneyTwoHandler();
		journeyThreeHandler = new zbJourneyThreeHandler();
		passportHandler = new zbPassPortHandler();
		elementTrialHandler = new zbElementTrialHandler();
		attrHandler = new zbAttrHandler();
		mechaHandler = new zbMechaHandler();
		wallHandler = new zbWallHandler();
		legendaryHandler = new zbLegendaryHandler();
		arenaHandler = new zbArenaHandler();
		pvpHandler = new zbPvpHandler();
		guardHandler = new zbFootholdHandler();
		crossLadderHandler = new zbCrossLadderHandler();
		geneHandler = new zbGeneHandler();
		treasureHandler = new zbTreasureHandler();
		mineWarHandler = new zbMineWarHandler();
		growthGiftHandler = new zbGrowthGiftHandler();
		mineWarHandler = new zbMineWarHandler();
		servantHandler = new zbServantHandler();
		crossPeakHandler = new zbCrossPeakHandler();
		reminderHandler = new zbReminderHandler();
		truckHandler = new zbTruckHandler();
		facilityHandler = new zbFacilityHandler();
		bossSubjectHandler = new zbBossSubjectHandler();
		wallSubjectHandler = new zbWallSubjectHandler();
		mechaSubjectHandler = new zbMechaSubjectHandler();
		notifySetHandler = new zbNotifySetHandler();
		seasonHandler = new zbSeasonHandler();
		tacticHandler = new zbTacticHandler();
		timeHandler.Init();
		dataHandler.Init();
		barHandler.Init();
		chatHandler.Init();
		friendHandler.Init();
		heroHandler.Init();
		equipHandler.Init();
		itemHandler.Init();
		mailHandler.Init();
		moneyHandler.Init();
		wallSubjectHandler.Init();
		mechaSubjectHandler.Init();
		newTagHandler.Init();
		redPointHandler.Init();
		shopHandler.Init();
		taskHandler.Init();
		levelUpHandler.Init();
		homelandHandler.Init();
		homeEditHandler.Init();
		funHandler.Init();
		legionHandler.Init();
		legionWarHandler.Init();
		vipHandler.Init();
		mainQuestHandler.Init();
		eliteQuestHandler.Init();
		endlessHordeHandler.Init();
		activityHandler.Init();
		operateHandler.Init();
		holidayHandler.Init();
		restaurantHandler.Init();
		endlessMineHandler.Init();
		snowVeilHandler.Init();
		worldDungeonHandler.Init();
		worldHandler.Init();
		worldLairHandler.Init();
		rankHandler.Init();
		worldBossHandler.Init();
		headHandler.Init();
		trialHandler.Init();
		guideHandler.Init();
		handbookHandler.Init();
		combatHandler.Init();
		suitGunHandler.Init();
		journeyOneHandler.Init();
		journeyTwoHandler.Init();
		journeyThreeHandler.Init();
		monsterHandler.Init();
		playerSuggestHandler.Init();
		funcUnLockHandler.Init();
		passportHandler.Init();
		elementTrialHandler.Init();
		attrHandler.Init();
		mechaHandler.Init();
		wallHandler.Init();
		legendaryHandler.Init();
		pvpHandler.Init();
		seasonHandler.Init();
		guardHandler.Init();
		crossLadderHandler.Init();
		geneHandler.Init();
		treasureHandler.Init();
		mineWarHandler.Init();
		growthGiftHandler.Init();
		mineWarHandler.Init();
		servantHandler.Init();
		crossPeakHandler.Init();
		redPointHandler.Init();
		truckHandler.Init();
		facilityHandler = new zbFacilityHandler();
		facilityHandler.Init();
		bossSubjectHandler.Init();
		notifySetHandler.Init();
		getWayHandler.Init();
		seasonHandler.Init();
		tacticHandler.Init();
		arenaHandler.Init();
	}

	public void Free()
	{
	}

	public void Update()
	{
		timeHandler.Update();
		levelUpHandler.Update();
		newTagHandler.Update();
		trialHandler.Update();
		homelandHandler.Update();
		worldHandler.Update();
		funcUnLockHandler.Update();
	}

	public void OnLoadingComplete()
	{
		m_bLoading = false;
		attrHandler.OnLoadingComplete();
		if (!Instance.funHandler.IsLockNoTip(EFuncType.GeneLock))
		{
			Instance.newTagHandler.AddNewFunc(54.ToString());
		}
	}

	public bool IsLoading()
	{
		return m_bLoading;
	}

	public string GetRoleName()
	{
		return zbServerData.sGameData.RoleBaseInfo.RoleName;
	}

	public int GetAvatarId()
	{
		return zbServerData.sGameData.RoleBaseInfo.AvatarId;
	}

	public long GetRoleId()
	{
		return zbServerData.sGameData.RoleBaseInfo.RoleId;
	}

	public int GetServerId()
	{
		return zbServerData.sGameData.RoleBaseInfo.ServerId;
	}

	public int GetLevel()
	{
		return zbServerData.sGameData.RoleBaseInfo.Level;
	}

	public int GetVipLevel()
	{
		return zbServerData.sGameData.RoleBaseInfo.VipLevel;
	}

	public int GetFightValue()
	{
		return zbServerData.sGameData.RoleBaseInfo.TeamAttack;
	}

	public int GetHeadId()
	{
		return zbServerData.sGameData.RoleBaseInfo.HeadId;
	}

	public int GetHeadFrameId()
	{
		return zbServerData.sGameData.RoleBaseInfo.HeadFrameId;
	}

	public int GetSuitId()
	{
		return zbServerData.sGameData.RoleBaseInfo.SuitId;
	}

	public int GetGunId()
	{
		return zbServerData.sGameData.RoleBaseInfo.GunId;
	}

	public int GetMechaId()
	{
		return zbServerData.sGameData.RoleBaseInfo.MechaId;
	}

	public void UpdateRoleBaseInfo(RoleBaseInfo _roleBaseInfo)
	{
		if (_roleBaseInfo != null)
		{
			_ = zbServerData.sGameData.RoleBaseInfo.Level;
			zbServerData.sGameData.RoleBaseInfo = _roleBaseInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_RoleBaseInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Head_Data);
		}
	}
}
