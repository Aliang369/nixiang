using Proto.Enums;
using Proto.Types;
using UnityEngine;
using zbType;

public static class zbLinkHelper
{
	public static void OpenLink(string linkKey, int param)
	{
		switch (linkKey)
		{
		case "main":
			break;
		case "homeMainUpgrade":
			break;
		case "bar1":
			OpenBar(0);
			break;
		case "bar2":
			OpenBar(1);
			break;
		case "shop1":
			OpenShop(0);
			break;
		case "shop2":
			OpenShop(1);
			break;
		case "shop3":
			OpenShop(2);
			break;
		case "shop4":
			OpenShop(3);
			break;
		case "shop5":
			OpenShop(4);
			break;
		case "idleGuide":
			OpenSweepGuide();
			break;
		case "teamEditGuide":
			OpenTeamEditGuide();
			break;
		case "mainQuest":
		case "mainQuestTarget":
			OpenMainQuest();
			break;
		case "eliteQuest":
			OpenEliteQuest();
			break;
		case "heroTrial":
			OpenHome();
			break;
		case "peerage":
			OpenPeeerage();
			break;
		case "task":
		case "dailyTask":
		case "dailyTaskTarget":
			OpenTask();
			break;
		case "passPort":
			OpenPassport();
			break;
		case "heroDecompose":
			zbUIMgr.Instance.ShowHeroDecompose();
			break;
		case "equipDecompose":
			zbUIMgr.Instance.ShowEquipDecompose();
			break;
		case "heroRarity":
			OpenHeroRarity();
			break;
		case "roomBuild":
			OpenRoomBuild();
			break;
		case "homeUnlock":
			OpenHome();
			break;
		case "homeCreate":
			OpenHomeCreate(param);
			break;
		case "home":
			OpenHome();
			break;
		case "legionBoss":
			OpenLegionChallenge();
			break;
		case "legionDinner":
			OpenLegionDinner();
			break;
		case "legionDonate":
			OpenLegion();
			break;
		case "world":
		case "worldEvent":
		case "worldRes":
			OpenWorld();
			break;
		case "worldLairGuide":
			OpenWorldLairGuide();
			break;
		case "worldLair":
			OpenWorldLair();
			break;
		case "worldBoss":
			OpenWorldBoss();
			break;
		case "worldBossGuide":
			OpenWorldBossGuide();
			break;
		case "worldBossRank":
			OpenWorldBoss();
			break;
		case "dailySign":
			OpenDailySign();
			break;
		case "tiliBank":
			OpenTiliBank();
			break;
		case "dailyPack":
			OpenVip(1);
			break;
		case "monthCard":
			OpenVip(3);
			break;
		case "recharge":
			OpenVip(2);
			break;
		case "voucherConsume":
			OpenVip(2, 1);
			break;
		case "friend":
			OpenFriend();
			break;
		case "equipUpgrade":
			OpenEquipUpgrade(1);
			break;
		case "equipSlotUpgrade":
			OpenEquipUpgrade(0);
			break;
		case "leader":
		case "gem":
		case "gemCompose":
			OpenLeader();
			break;
		case "equip":
		case "leader_equip":
			OpenLeader(1);
			break;
		case "formation":
			zbUIMgr.Instance.ShowFormation();
			break;
		case "hero":
			zbUIMgr.Instance.ShowHeroList();
			break;
		case "heroUpgrade":
			OpenHeroUpgrade();
			break;
		case "heroAwaken":
			OpenHeroAwaken();
			break;
		case "heroGene":
			OpenHeroGene();
			break;
		case "heroFollow":
			OpenHeroFollow();
			break;
		case "name":
			OpenChangeName();
			break;
		case "wallIdle":
			OpenWallIdle();
			break;
		case "elementTrial":
			OpenElementTrial();
			break;
		case "passportMainQuest":
			OpenPassportMainQuest();
			break;
		case "endlessHorde":
			OpenEndlessHorde();
			break;
		case "mecha":
		case "mechaSkill":
			OpenMecha(param);
			break;
		case "journeyOneBar":
			OpenJourneyOne(EJourneyOneMenu.Bar);
			break;
		case "journeyOneRecharge":
			OpenJourneyOne(EJourneyOneMenu.RechargeSum);
			break;
		case "journeyOneExchange":
			OpenJourneyOne(EJourneyOneMenu.Shop);
			break;
		case "journeyOneZhanLing":
			OpenJourneyOne(EJourneyOneMenu.ZhanLing);
			break;
		case "journeyOneSign":
			OpenJourneyOne(EJourneyOneMenu.XunLuo);
			break;
		case "journeyTwoBar":
			OpenJourneyTwo(EJourneyTwoMenu.Bar);
			break;
		case "journeyTwoDiamondCost":
			OpenJourneyTwo(EJourneyTwoMenu.DiamondCost);
			break;
		case "journeyTwoExchange":
			OpenJourneyTwo(EJourneyTwoMenu.Exchange);
			break;
		case "journeyTwoActive":
			OpenJourneyTwo(EJourneyTwoMenu.Active);
			break;
		case "journeyTwoZhanLing":
			OpenJourneyTwo(EJourneyTwoMenu.ZhanLing);
			break;
		case "journeyThreeBar":
			OpenJourneyThree(EJourneyThreeMenu.Bar);
			break;
		case "journeyThreeDrawCount":
			OpenJourneyThree(EJourneyThreeMenu.DrawCount);
			break;
		case "journeyThreeExchange":
			OpenJourneyThree(EJourneyThreeMenu.Exchange);
			break;
		case "journeyThreeSign":
			OpenJourneyThree(EJourneyThreeMenu.Sign);
			break;
		case "journeyThreeZhanLing":
			OpenJourneyThree(EJourneyThreeMenu.ZhanLing);
			break;
		case "handbook":
			OpenHandbookMod();
			break;
		case "wall":
			OpenWall();
			break;
		case "wall_skill":
			OpenWall(2);
			break;
		case "legendary":
			OpenLegendary();
			break;
		case "legendaryZhanling":
			OpenLegendaryZhanling();
			break;
		case "arena":
			OpenArena();
			break;
		case "arenaGift":
			OpenArenaGift();
			break;
		case "crossLadder":
			OpenCrossLadder();
			break;
		case "crossPeak":
			OpenCrossPeak();
			break;
		case "treasure":
			OpenTreasure();
			break;
		case "treasureBar":
			OpenTreasure(1);
			break;
		case "treasurePrivilege":
			zbUIMgr.Instance.ShowTreasurePrivilege();
			break;
		case "mineWar":
			OpenMineWar();
			break;
		case "arenaGuide":
			OpenArenaGuide();
			break;
		case "geneCompose":
			OpenGeneCompose();
			break;
		case "playerSuit":
			zbUIMgr.Instance.ShowPlayerSuit();
			break;
		case "playerGun":
			zbUIMgr.Instance.ShowPlayerGun();
			break;
		case "goldExchange":
			OpenMoneyExchange(EMoneyType.Gold);
			break;
		case "tiliExchange":
			OpenMoneyExchange(EMoneyType.Tili);
			break;
		case "truck":
		case "truckMine":
			OpenTruck();
			break;
		case "truckTheir":
			OpenTruck(0, fromTask: true);
			break;
		case "legionWar":
			OpenLegionWar();
			break;
		case "facility":
		case "facilityMine":
			OpenFacility();
			break;
		case "facilityTheir":
			OpenFacility(1);
			break;
		case "foothold":
			OpenFoothold();
			break;
		case "guard":
			OpenGuard();
			break;
		case "vip":
			zbUIMgr.Instance.ShowVipTequanWnd();
			break;
		case "sign":
			OpenDailySign();
			break;
		case "firstRecharge":
			OpenFirstRecharge();
			break;
		case "openServerRank":
			OpenServerRank();
			break;
		}
	}

	public static void OpenChangeName()
	{
		zbUIMgr.Instance.ShowResetName();
	}

	public static void OpenWarRoom(int typeId)
	{
		zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(typeId);
		if (recordByKey != null)
		{
			WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)recordByKey.subType);
			if (warRoomDetailBySkillType != null && warRoomDetailBySkillType.HeroTid > 0)
			{
				zbHeroCsvRecord recordByKey2 = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(warRoomDetailBySkillType.HeroTid);
				zbUIMgr.Instance.ShowHeroDetail(recordByKey2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10727"));
			}
		}
	}

	public static void OpenHome()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Homeland))
		{
			zbUIMgr.Instance.ShowHome();
		}
	}

	public static void OpenHomeCreate(int param)
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Homeland))
		{
			zbUIMgr.Instance.ShowHome();
			zbSelf.Instance.homelandHandler.PositionByHomeRoomId(param);
		}
	}

	public static void OpenMainQuest()
	{
		zbUIMgr.Instance.ShowMainLevel(0, zbServerData.sGameData.MainQuestInfo.NextLevel);
	}

	public static void OpenEliteQuest()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.EliteQuest))
		{
			zbUIMgr.Instance.ShowEliteQuest(zbServerData.sGameData.EliteQuestInfo.NextLevel);
		}
	}

	public static void OpenPeeerage()
	{
		zbUIMgr.Instance.ShowPeerage();
	}

	public static void OpenTask()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Task))
		{
			zbUIMgr.Instance.ShowTask();
		}
	}

	public static void OpenPassport()
	{
		zbUIMgr.Instance.ShowVip(4, 1);
	}

	public static void OpenOperate()
	{
		zbWsMsgHandler.Instance.Operate_Data_Req(open: true);
	}

	public static void OpenBar(int tabIndex)
	{
		zbUIMgr.Instance.ShowLottery(tabIndex);
	}

	public static void OpenShop(int tabIndex)
	{
		EFuncType funcTypeByTab = zbSelf.Instance.shopHandler.GetFuncTypeByTab(tabIndex);
		if (!zbSelf.Instance.funHandler.IsLockAndTip(funcTypeByTab))
		{
			zbUIMgr.Instance.ShowShop(tabIndex);
		}
	}

	public static void OpenRoomUpgrade(HomelandRoomDetail roomData)
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Homeland))
		{
			zbUIMgr.Instance.ShowRoomDetail(roomData);
		}
	}

	public static void OpenRoomBuild()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Homeland))
		{
			zbUIMgr.Instance.ShowHomeCreate();
		}
	}

	public static void OpenHeroUpgrade()
	{
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(10100);
		zbUIMgr.Instance.ShowHero(recordByKey);
	}

	public static void OpenHeroAwaken()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.HeroAwaken))
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(10100);
			zbUIMgr.Instance.ShowHero(recordByKey, 1);
		}
	}

	public static void OpenHeroGene()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.GeneLock))
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(10100);
			zbUIMgr.Instance.ShowHero(recordByKey, 2);
		}
	}

	public static void OpenHeroFollow()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.HeroFollow))
		{
			zbUIMgr.Instance.ShowHeroList();
		}
	}

	public static void OpenHeroRarity()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.HeroRarity))
		{
			zbUIMgr.Instance.ShowHeroRarity();
		}
	}

	public static void OpenGeneCompose()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.GeneLock))
		{
			zbUIMgr.Instance.ShowGene();
		}
	}

	public static void OpenLeader(int tabIndex = 0)
	{
		zbUIMgr.Instance.ShowLeader(tabIndex);
	}

	public static void OpenEquipUpgrade(int index)
	{
		zbUIMgr.Instance.ShowEquipUpgrade(EEquipType.Neuron, index);
	}

	public static void OpenWorldLair()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.WorldLair))
		{
			WorldLairInfo worldLairInfo = zbServerData.sGameData.WorldLairInfo;
			if (worldLairInfo != null && worldLairInfo.SelfLairInfo != null)
			{
				zbUIMgr.Instance.ShowWorldLairFight(worldLairInfo.SelfLairInfo);
				return;
			}
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10695"));
			zbUIMgr.Instance.ShowWorldLair();
		}
	}

	public static void OpenWorldLairGuide()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.WorldLair))
		{
			zbUIMgr.Instance.ShowChallenge(EFuncType.WorldLair);
			(zbUIMgr.Instance.GetWndById(EWndId.Challenge) as zbChallengeWnd).ShowGuideByFuncType(EFuncType.WorldLair);
		}
	}

	public static void OpenSweepGuide()
	{
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Idle))
		{
			zbUIMgr.Instance.ShowMainLevel();
			zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
			if ((Object)(object)zbMainLevelWnd2 != (Object)null)
			{
				zbMainLevelWnd2.ShowSweepFinger();
			}
		}
	}

	public static void OpenTeamEditGuide()
	{
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena))
		{
			zbUIMgr.Instance.ShowArenaMain();
			zbArenaMainWnd zbArenaMainWnd2 = zbUIMgr.Instance.GetWndById(EWndId.ArenaMain) as zbArenaMainWnd;
			if ((Object)(object)zbArenaMainWnd2 != (Object)null)
			{
				zbArenaMainWnd2.OnShowEditTeamGuide();
			}
		}
	}

	public static void OpenFriend()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Friends))
		{
			zbWsMsgHandler.Instance.Friends_List_Req();
			zbUIMgr.Instance.ShowFriends();
		}
	}

	public static void OpenWorldBoss()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.WorldBoss))
		{
			zbWsMsgHandler.Instance.WorldBoss_Data_Req(open: true);
		}
	}

	public static void OpenWorldBossGuide()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.WorldBoss))
		{
			zbUIMgr.Instance.ShowChallenge(EFuncType.WorldBoss);
			(zbUIMgr.Instance.GetWndById(EWndId.Challenge) as zbChallengeWnd).ShowGuideByFuncType(EFuncType.WorldBoss);
		}
	}

	public static void OpenLegion()
	{
		if (zbSelf.Instance.newTagHandler.isFirstUnLockLegion())
		{
			zbSelf.Instance.newTagHandler.SetFirstUnlockLegion(isFirstUnLockLegion: false);
		}
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Legion))
		{
			if (zbSelf.Instance.legionHandler.HasJoinLegion())
			{
				zbWsMsgHandler.Instance.Legion_Data_Req(bOpenLegionWnd: true);
			}
			else
			{
				zbUIMgr.Instance.ShowLegionRecommend();
			}
		}
	}

	public static void OpenLegionChallenge()
	{
		if (zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.LegionBoss))
		{
			return;
		}
		if (zbSelf.Instance.legionHandler.HasJoinLegion())
		{
			int level = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Level;
			int legionLvl = zbDataConfig.Instance.legionUnlockCsvTable.GetRecordByKey(3).legionLvl;
			if (level >= legionLvl)
			{
				zbUIMgr.Instance.ShowLegionBoss();
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10735"), legionLvl));
			}
		}
		else
		{
			OpenLegion();
		}
	}

	public static void OpenLegionDinner()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Legion) && zbSelf.Instance.legionHandler.HasJoinLegion())
		{
			int level = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Level;
			int legionLvl = zbDataConfig.Instance.legionUnlockCsvTable.GetRecordByKey(1).legionLvl;
			if (level >= legionLvl)
			{
				zbUIMgr.Instance.ShowLegionDinner();
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10735"), legionLvl));
			}
		}
	}

	public static void OpenChat()
	{
		zbUIMgr.Instance.ShowChat();
	}

	public static void OpenMail()
	{
		zbWsMsgHandler.Instance.Mail_GetList_Req(bOpenMailWnd: true);
	}

	public static void OpenVip(int tabIndex = 0, int subTabIndex = 0)
	{
		zbUIMgr.Instance.ShowVip(tabIndex, subTabIndex);
	}

	public static void OpenRank()
	{
		zbWsMsgHandler.Instance.Rank_List_Req(ERankType.MainQuest, openRank: true);
	}

	public static void OpenServerRank()
	{
		if (zbSelf.Instance.rankHandler.IsOpenServerOpen())
		{
			zbWsMsgHandler.Instance.OpenServer_RankList_Req(ERankType.MainQuest);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo("该活动已结束");
		}
	}

	public static void OpenHandbook()
	{
		zbWsMsgHandler.Instance.Handbook_Data_Req(isOpen: true);
	}

	public static void OpenHandbookMod()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.HandbookMod))
		{
			zbWsMsgHandler.Instance.Handbook_Data_Req(isOpen: true, 1);
		}
	}

	public static void OpenWall(int tabIndex = 0)
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.WallDefense))
		{
			zbWsMsgHandler.Instance.Wall_Data_Req(open: true, tabIndex);
		}
	}

	public static void OpenDailySign()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.DailySign))
		{
			zbUIMgr.Instance.ShowDailySign();
		}
	}

	public static void OpenTiliBank()
	{
		zbUIMgr.Instance.ShowTiliBank();
	}

	public static void OpenWorld()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Explore))
		{
			zbWsMsgHandler.Instance.World_Data_Req(open: true);
		}
	}

	public static void OpenHeroTrial()
	{
		zbUIMgr.Instance.ShowHeroTrial();
	}

	public static void OpenWallIdle()
	{
		if (zbServerData.sGameData.MainQuestInfo.MainLevel < 1)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10545"));
		}
		else if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Idle))
		{
			zbWsMsgHandler.Instance.Idle_Data_Req(open: true);
		}
	}

	public static void OpenLegendary()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Legendary))
		{
			zbWsMsgHandler.Instance.Legendary_Data_Req(open: true);
		}
	}

	public static void OpenLegendaryZhanling()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Legendary))
		{
			zbUIMgr.Instance.ShowLegendaryZhanLing();
		}
	}

	public static void OpenElementTrial()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.ElementTrial))
		{
			zbUIMgr.Instance.ShowElementTrial();
		}
	}

	public static void OpenPassportMainQuest()
	{
		zbUIMgr.Instance.ShowVip(4);
	}

	public static void OpenEndlessHorde()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.EndlessHorde))
		{
			zbWsMsgHandler.Instance.EndlessHorde_Data_Req(openWnd: true);
		}
	}

	public static void OpenMecha(int mechaId)
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Mecha))
		{
			int num = mechaId;
			if (num == 0)
			{
				num = ((zbSelf.Instance.GetMechaId() > 0) ? zbSelf.Instance.GetMechaId() : 101);
			}
			zbMechaCsvRecord mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(num);
			if (mechaRecord != null)
			{
				zbUIMgr.Instance.ShowMecha(mechaRecord);
			}
		}
	}

	public static void OpenArena()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Arena))
		{
			zbWsMsgHandler.Instance.Arena_Data_Req(openWnd: true);
		}
	}

	public static void OpenArenaGift()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Arena))
		{
			zbUIMgr.Instance.ShowArenaGift();
		}
	}

	public static void OpenArenaGuide()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Arena))
		{
			zbWsMsgHandler.Instance.Arena_Data_Req(openWnd: true, showGuide: true);
		}
	}

	public static void OpenFirstRecharge()
	{
		if (!zbSelf.Instance.activityHandler.IsFirstRechargeEnd())
		{
			zbUIMgr.Instance.ShowFirstRecharge();
		}
	}

	public static void OpenCrossLadder()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.CrossLadder) && zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen(needBubble: true))
		{
			if (zbSelf.Instance.crossLadderHandler.GetNextOpenLeftTime() > 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10943"));
			}
			else
			{
				zbWsMsgHandler.Instance.CrossLadder_Data_Req(openWnd: true);
			}
		}
	}

	public static void OpenCrossPeak()
	{
		if (zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.CrossLadder))
		{
			return;
		}
		if (zbServerData.sGameData?.CrossLadderInfo?.AchieveInfo.LadderId >= zbSelf.Instance.crossLadderHandler.maxLadderId)
		{
			if (zbSelf.Instance.crossLadderHandler.GetNextOpenLeftTime() > 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10943"));
				return;
			}
			long num = zbGameSave.Instance.GetLong("OpenCrossPeak_" + zbSelf.Instance.GetRoleId(), 0L);
			if (num > 0 && zbUtil.IsCurrentMonth(num))
			{
				zbWsMsgHandler.Instance.CrossPeak_Data_Req(openWnd: true);
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo("尚未开启巅峰赛");
		}
	}

	public static void OpenTreasure(int tab = 0)
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Treasure))
		{
			zbWsMsgHandler.Instance.Treasure_Data_Req(openWnd: true, tab);
		}
	}

	public static void OpenMineWar()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.MineWar))
		{
			zbWsMsgHandler.Instance.MineWar_Data_Req(openWnd: true);
		}
	}

	public static void OpenMoneyExchange(EMoneyType moneyType)
	{
		zbMoneyExchangeCsvRecord recordByKey = zbDataConfig.Instance.moneyExchangeCsvTable.GetRecordByKey((int)moneyType);
		zbUIMgr.Instance.ShowMoneyExchange(recordByKey);
	}

	public static void OpenTruck(int tabIndex = 1, bool fromTask = false)
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Truck))
		{
			zbWsMsgHandler.Instance.Truck_MatchList_Req(openWnd: true);
			zbWsMsgHandler.Instance.Truck_Data_Req(openWnd: true, tabIndex, fromTask);
		}
	}

	public static void OpenLegionWar()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.LegionWar))
		{
			OpenLegion();
		}
	}

	public static void OpenFacility(int tabIndex = 0)
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Facility))
		{
			zbUIMgr.Instance.ShowFacilityHub(tabIndex);
		}
	}

	public static void OpenFoothold()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Arena))
		{
			zbWsMsgHandler.Instance.Foothold_Data_Req(zbServerData.sGameData.RoleBaseInfo.FootholdId);
		}
	}

	public static void OpenGuard()
	{
	}

	public static void OpenJourneyMain()
	{
		if (zbSelf.Instance.journeyOneHandler.IsActive())
		{
			if (zbSelf.Instance.newTagHandler.IsJourneyMovieUnSeen(1))
			{
				zbUIMgr.Instance.ShowJourneyMovie(1);
			}
			else
			{
				zbUIMgr.Instance.ShowJourneyOneMain();
			}
		}
		else if (zbSelf.Instance.journeyTwoHandler.IsActive())
		{
			if (zbSelf.Instance.newTagHandler.IsJourneyMovieUnSeen(2))
			{
				zbUIMgr.Instance.ShowJourneyMovie(2);
			}
			else
			{
				zbUIMgr.Instance.ShowJourneyTwoMain();
			}
		}
		else if (zbSelf.Instance.journeyThreeHandler.IsActive())
		{
			if (zbSelf.Instance.newTagHandler.IsJourneyMovieUnSeen(3))
			{
				zbUIMgr.Instance.ShowJourneyMovie(3);
			}
			else
			{
				zbUIMgr.Instance.ShowJourneyThreeMain();
			}
		}
	}

	public static void OpenJourneyOne(EJourneyOneMenu journeyType)
	{
		if (!zbSelf.Instance.journeyOneHandler.IsActive())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			return;
		}
		switch (journeyType)
		{
		case EJourneyOneMenu.RechargeSum:
			zbUIMgr.Instance.ShowJourneyOneRechargeSum();
			break;
		case EJourneyOneMenu.Shop:
			zbUIMgr.Instance.ShowJourneyOneShop();
			break;
		case EJourneyOneMenu.Bar:
			zbUIMgr.Instance.ShowJourneyOneBar();
			break;
		case EJourneyOneMenu.XunLuo:
			zbUIMgr.Instance.ShowJourneyOneXunLuo();
			break;
		case EJourneyOneMenu.ZhanLing:
			zbUIMgr.Instance.ShowJourneyOneZhanling();
			break;
		}
	}

	public static void OpenJourneyTwo(EJourneyTwoMenu journeyType)
	{
		if (!zbSelf.Instance.journeyTwoHandler.IsActive())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			return;
		}
		switch (journeyType)
		{
		case EJourneyTwoMenu.Bar:
			zbUIMgr.Instance.ShowJourneyTwoBar();
			break;
		case EJourneyTwoMenu.DiamondCost:
			zbUIMgr.Instance.ShowJourneyTwoDiamondCost();
			break;
		case EJourneyTwoMenu.Exchange:
			zbUIMgr.Instance.ShowJourneyTwoExchange();
			break;
		case EJourneyTwoMenu.Active:
			zbUIMgr.Instance.ShowJourneyTwoActive();
			break;
		case EJourneyTwoMenu.ZhanLing:
			OpenTruck();
			break;
		}
	}

	public static void OpenJourneyThree(EJourneyThreeMenu journeyType)
	{
		if (!zbSelf.Instance.journeyThreeHandler.IsActive())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			return;
		}
		switch (journeyType)
		{
		case EJourneyThreeMenu.Bar:
			zbUIMgr.Instance.ShowJourneyThreeBar();
			break;
		case EJourneyThreeMenu.DrawCount:
			zbUIMgr.Instance.ShowJourneyThreeDrawCount();
			break;
		case EJourneyThreeMenu.Exchange:
			zbUIMgr.Instance.ShowJourneyThreeExchange();
			break;
		case EJourneyThreeMenu.Sign:
			zbUIMgr.Instance.ShowJourneyThreeSign();
			break;
		case EJourneyThreeMenu.ZhanLing:
			OpenFacility();
			break;
		}
	}

	public static void OpenOperateByType(EOperateType operateType, EResourceType type, long id)
	{
		zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey((int)operateType);
		switch (operateType)
		{
		case EOperateType.RechargeDaily:
			if (zbSelf.Instance.holidayHandler.uuidRechargeDaily.Count > 0)
			{
				zbUIMgr.Instance.ShowRechargeDaily(zbSelf.Instance.holidayHandler.uuidRechargeDaily[0], recordByKey);
			}
			else if (zbSelf.Instance.operateHandler.uuidRechargeDaily.Count > 0)
			{
				zbUIMgr.Instance.ShowRechargeDaily(zbSelf.Instance.operateHandler.uuidRechargeDaily[0], recordByKey);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.RechargeDailySum:
			if (zbSelf.Instance.holidayHandler.uuidRechargeDailySum.Count > 0)
			{
				zbUIMgr.Instance.ShowRechargeDailySum(zbSelf.Instance.holidayHandler.uuidRechargeDailySum[0], recordByKey);
			}
			else if (zbSelf.Instance.operateHandler.uuidRechargeDailySum.Count > 0)
			{
				zbUIMgr.Instance.ShowRechargeDailySum(zbSelf.Instance.operateHandler.uuidRechargeDailySum[0], recordByKey);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.RechargeSum:
			if (zbSelf.Instance.holidayHandler.uuidRechargeSum.Count > 0)
			{
				zbUIMgr.Instance.ShowRechargeSum(zbSelf.Instance.holidayHandler.uuidRechargeSum[0], recordByKey);
			}
			else if (zbSelf.Instance.operateHandler.uuidRechargeSum.Count > 0)
			{
				zbUIMgr.Instance.ShowRechargeSum(zbSelf.Instance.operateHandler.uuidRechargeSum[0], recordByKey);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.RechargeGift:
			if (zbSelf.Instance.holidayHandler.uuidRechargeGift.Count > 0)
			{
				zbUIMgr.Instance.ShowRechargeGift(zbSelf.Instance.holidayHandler.uuidRechargeGift[0]);
			}
			else if (zbSelf.Instance.operateHandler.uuidRechargeGift.Count > 0)
			{
				zbUIMgr.Instance.ShowVip();
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.ConsumeDailySum:
			if (zbSelf.Instance.holidayHandler.uuidConsumeDailySum.Count > 0)
			{
				zbUIMgr.Instance.ShowConsumeDailySum(zbSelf.Instance.holidayHandler.uuidConsumeDailySum[0], recordByKey);
			}
			else if (zbSelf.Instance.operateHandler.uuidConsumeDailySum.Count > 0)
			{
				zbUIMgr.Instance.ShowConsumeDailySum(zbSelf.Instance.operateHandler.uuidConsumeDailySum[0], recordByKey);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.ConsumeSum:
			if (zbSelf.Instance.holidayHandler.uuidConsumeSum.Count > 0)
			{
				zbUIMgr.Instance.ShowConsumeSum(zbSelf.Instance.holidayHandler.uuidConsumeSum[0], recordByKey);
			}
			else if (zbSelf.Instance.operateHandler.uuidConsumeSum.Count > 0)
			{
				zbUIMgr.Instance.ShowConsumeSum(zbSelf.Instance.operateHandler.uuidConsumeSum[0], recordByKey);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.LimitTimeHero:
			zbUIMgr.Instance.ShowLottery(2);
			break;
		case EOperateType.LimitTimeShop:
			if (zbSelf.Instance.holidayHandler.uuidLimitTimeShop.Count > 0)
			{
				LimitTimeShopDetail timeShopInfoByUUID = zbSelf.Instance.operateHandler.GetTimeShopInfoByUUID(zbSelf.Instance.holidayHandler.uuidLimitTimeShop[0]);
				zbUIMgr.Instance.ShowLimitTimeShop(timeShopInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidLimitTimeShop.Count > 0)
			{
				LimitTimeShopDetail timeShopInfoByUUID2 = zbSelf.Instance.operateHandler.GetTimeShopInfoByUUID(zbSelf.Instance.operateHandler.uuidLimitTimeShop[0]);
				zbUIMgr.Instance.ShowLimitTimeShop(timeShopInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.LoginSum:
			if (zbSelf.Instance.holidayHandler.uuidLoginSum.Count > 0)
			{
				LoginSumDetail loginSumInfoByUUID = zbSelf.Instance.operateHandler.GetLoginSumInfoByUUID(zbSelf.Instance.holidayHandler.uuidLoginSum[0]);
				zbUIMgr.Instance.ShowLoginSum(loginSumInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidLoginSum.Count > 0)
			{
				LoginSumDetail loginSumInfoByUUID2 = zbSelf.Instance.operateHandler.GetLoginSumInfoByUUID(zbSelf.Instance.operateHandler.uuidLoginSum[0]);
				zbUIMgr.Instance.ShowLoginSum(loginSumInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.DailyActive:
			if (zbSelf.Instance.holidayHandler.uuidDailyActive.Count > 0)
			{
				zbUIMgr.Instance.ShowDailyActive(zbSelf.Instance.holidayHandler.uuidDailyActive[0], recordByKey);
			}
			else if (zbSelf.Instance.operateHandler.uuidDailyActive.Count > 0)
			{
				zbUIMgr.Instance.ShowDailyActive(zbSelf.Instance.operateHandler.uuidDailyActive[0], recordByKey);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.BarDrawNormal:
		case EOperateType.BarDrawSpecial:
			if (zbSelf.Instance.holidayHandler.uuidBarDraw.Count > 0)
			{
				zbUIMgr.Instance.ShowBarDrawNormalSum(zbSelf.Instance.holidayHandler.uuidBarDraw[0], recordByKey);
			}
			else if (zbSelf.Instance.operateHandler.uuidBarDraw.Count > 0)
			{
				zbUIMgr.Instance.ShowBarDrawNormalSum(zbSelf.Instance.operateHandler.uuidBarDraw[0], recordByKey);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.PropExchange:
			if (zbSelf.Instance.holidayHandler.uuidPropExchange.Count > 0)
			{
				PropExchangeDetail exchangeInfoByUUID = zbSelf.Instance.operateHandler.GetExchangeInfoByUUID(zbSelf.Instance.holidayHandler.uuidPropExchange[0]);
				zbUIMgr.Instance.ShowPropExchange(exchangeInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidPropExchange.Count > 0)
			{
				PropExchangeDetail exchangeInfoByUUID2 = zbSelf.Instance.operateHandler.GetExchangeInfoByUUID(zbSelf.Instance.operateHandler.uuidPropExchange[0]);
				zbUIMgr.Instance.ShowPropExchange(exchangeInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.ScratchCard:
			if (zbSelf.Instance.holidayHandler.uuidScratchCard.Count > 0)
			{
				ScratchCardDetail scratchCardInfoByUUID = zbSelf.Instance.operateHandler.GetScratchCardInfoByUUID(zbSelf.Instance.holidayHandler.uuidScratchCard[0]);
				zbUIMgr.Instance.ShowScratchCard(scratchCardInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidScratchCard.Count > 0)
			{
				ScratchCardDetail scratchCardInfoByUUID2 = zbSelf.Instance.operateHandler.GetScratchCardInfoByUUID(zbSelf.Instance.operateHandler.uuidScratchCard[0]);
				zbUIMgr.Instance.ShowScratchCard(scratchCardInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.JiZi:
			if (zbSelf.Instance.holidayHandler.uuidJiZi.Count > 0)
			{
				JiZiDetail jiZiYouLiByUUID = zbSelf.Instance.operateHandler.GetJiZiYouLiByUUID(zbSelf.Instance.holidayHandler.uuidJiZi[0]);
				zbUIMgr.Instance.ShowJizi(jiZiYouLiByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidJiZi.Count > 0)
			{
				JiZiDetail jiZiYouLiByUUID2 = zbSelf.Instance.operateHandler.GetJiZiYouLiByUUID(zbSelf.Instance.operateHandler.uuidJiZi[0]);
				zbUIMgr.Instance.ShowJizi(jiZiYouLiByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.CipherBox:
			if (zbSelf.Instance.holidayHandler.uuidCipherBox.Count > 0)
			{
				CipherBoxDetail cipherBoxInfoByUUID = zbSelf.Instance.operateHandler.GetCipherBoxInfoByUUID(zbSelf.Instance.holidayHandler.uuidCipherBox[0]);
				zbUIMgr.Instance.ShowCipherBox(cipherBoxInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidCipherBox.Count > 0)
			{
				CipherBoxDetail cipherBoxInfoByUUID2 = zbSelf.Instance.operateHandler.GetCipherBoxInfoByUUID(zbSelf.Instance.operateHandler.uuidCipherBox[0]);
				zbUIMgr.Instance.ShowCipherBox(cipherBoxInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.BarrageCmd:
			if (zbSelf.Instance.holidayHandler.uuidRedBag.Count > 0)
			{
				zbUIMgr.Instance.ShowBarrageCmd(zbSelf.Instance.holidayHandler.uuidRedBag[0], recordByKey);
			}
			else if (zbSelf.Instance.operateHandler.uuidRedBag.Count > 0)
			{
				zbUIMgr.Instance.ShowBarrageCmd(zbSelf.Instance.operateHandler.uuidRedBag[0], recordByKey);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.LimitTimeCatcher:
			if (zbSelf.Instance.holidayHandler.uuidTimeCatcher.Count > 0)
			{
				LimitTimeCatcherDetail timeCatcherInfoByUUID = zbSelf.Instance.operateHandler.GetTimeCatcherInfoByUUID(zbSelf.Instance.holidayHandler.uuidTimeCatcher[0]);
				zbUIMgr.Instance.ShowTimeCatcher(timeCatcherInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidTimeCatcher.Count > 0)
			{
				LimitTimeCatcherDetail timeCatcherInfoByUUID2 = zbSelf.Instance.operateHandler.GetTimeCatcherInfoByUUID(zbSelf.Instance.operateHandler.uuidTimeCatcher[0]);
				zbUIMgr.Instance.ShowTimeCatcher(timeCatcherInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.HolidayTask:
			zbWsMsgHandler.Instance.Operate_HolidayTaskData_Req(openWnd: true);
			break;
		case EOperateType.PrayLantern:
			if (zbSelf.Instance.holidayHandler.uuidPrayLantern.Count > 0)
			{
				PrayLanternDetail prayLanternInfoByUUID = zbSelf.Instance.operateHandler.GetPrayLanternInfoByUUID(zbSelf.Instance.holidayHandler.uuidPrayLantern[0]);
				zbUIMgr.Instance.ShowPrayLantern(prayLanternInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidPrayLantern.Count > 0)
			{
				PrayLanternDetail prayLanternInfoByUUID2 = zbSelf.Instance.operateHandler.GetPrayLanternInfoByUUID(zbSelf.Instance.operateHandler.uuidPrayLantern[0]);
				zbUIMgr.Instance.ShowPrayLantern(prayLanternInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.ResourceExchange:
			if (zbSelf.Instance.holidayHandler.uuidResourceExchange.Count > 0)
			{
				ResourceExchangeDetail resourceExchangeInfoByUUID = zbSelf.Instance.operateHandler.GetResourceExchangeInfoByUUID(zbSelf.Instance.holidayHandler.uuidResourceExchange[0]);
				zbUIMgr.Instance.ShowResourceExchange(resourceExchangeInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidResourceExchange.Count > 0)
			{
				ResourceExchangeDetail resourceExchangeInfoByUUID2 = zbSelf.Instance.operateHandler.GetResourceExchangeInfoByUUID(zbSelf.Instance.operateHandler.uuidResourceExchange[0]);
				zbUIMgr.Instance.ShowResourceExchange(resourceExchangeInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.Fusion:
			zbWsMsgHandler.Instance.Operate_FusionData_Req();
			break;
		case EOperateType.InfiniteGift:
			if (zbSelf.Instance.holidayHandler.uuidInfiniteGift.Count > 0)
			{
				InfiniteGiftDetail infiniteGiftInfoByUUID = zbSelf.Instance.operateHandler.GetInfiniteGiftInfoByUUID(zbSelf.Instance.holidayHandler.uuidInfiniteGift[0]);
				zbUIMgr.Instance.ShowInfiniteGift(infiniteGiftInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidInfiniteGift.Count > 0)
			{
				InfiniteGiftDetail infiniteGiftInfoByUUID2 = zbSelf.Instance.operateHandler.GetInfiniteGiftInfoByUUID(zbSelf.Instance.operateHandler.uuidInfiniteGift[0]);
				zbUIMgr.Instance.ShowInfiniteGift(infiniteGiftInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.LimitTimeRoulette:
			if (zbSelf.Instance.holidayHandler.uuidLimitTimeRoulette.Count > 0)
			{
				LimitTimeRouletteDetail limitTimeRouletteInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeRouletteInfoByUUID(zbSelf.Instance.holidayHandler.uuidLimitTimeRoulette[0]);
				zbUIMgr.Instance.ShowLimitTimeRoulette(limitTimeRouletteInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidLimitTimeRoulette.Count > 0)
			{
				LimitTimeRouletteDetail limitTimeRouletteInfoByUUID2 = zbSelf.Instance.operateHandler.GetLimitTimeRouletteInfoByUUID(zbSelf.Instance.operateHandler.uuidLimitTimeRoulette[0]);
				zbUIMgr.Instance.ShowLimitTimeRoulette(limitTimeRouletteInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.LimitTimeTreasure:
			if (zbSelf.Instance.holidayHandler.uuidLimitTimeTreasure.Count > 0)
			{
				LimitTimeRouletteDetail limitTimeTreasureInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeTreasureInfoByUUID(zbSelf.Instance.holidayHandler.uuidLimitTimeTreasure[0]);
				zbUIMgr.Instance.ShowLimitTimeRoulette(limitTimeTreasureInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidLimitTimeTreasure.Count > 0)
			{
				LimitTimeRouletteDetail limitTimeTreasureInfoByUUID2 = zbSelf.Instance.operateHandler.GetLimitTimeTreasureInfoByUUID(zbSelf.Instance.operateHandler.uuidLimitTimeTreasure[0]);
				zbUIMgr.Instance.ShowLimitTimeRoulette(limitTimeTreasureInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case EOperateType.Restaurant:
		{
			foreach (RestaurantDetail restaurant in zbServerData.sGameData.OperateInfo.RestaurantList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(restaurant.BaseInfo))
				{
					zbUIMgr.Instance.ShowRestaurant(restaurant);
					break;
				}
			}
			break;
		}
		case EOperateType.ThemeTask:
		{
			foreach (ThemeTaskDetail themeTask in zbServerData.sGameData.OperateInfo.ThemeTaskList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(themeTask.BaseInfo))
				{
					zbUIMgr.Instance.ShowThemeTask(themeTask);
					break;
				}
			}
			break;
		}
		case EOperateType.EndlessMine:
			if (zbSelf.Instance.endlessMineHandler.GetEndlessMineDetail() != null)
			{
				zbWsMsgHandler.Instance.EndlessMine_Data_Req(openWnd: true);
			}
			break;
		case EOperateType.SnowVeil:
		{
			foreach (SnowVeilDetail snowVeil in zbServerData.sGameData.OperateInfo.SnowVeilList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(snowVeil.BaseInfo))
				{
					zbUIMgr.Instance.ShowSnowVeil(snowVeil);
					break;
				}
			}
			break;
		}
		case EOperateType.ZhanLingKill:
		{
			foreach (ZhanLingKillDetail zhanLingKill in zbServerData.sGameData.OperateInfo.ZhanLingKillList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(zhanLingKill.BaseInfo))
				{
					zbUIMgr.Instance.ShowZhanLingKill(zhanLingKill);
					break;
				}
			}
			break;
		}
		case EOperateType.BossSubject:
		{
			BossSubjectDetail bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
			if (bossSubjectDetail != null)
			{
				zbUIMgr.Instance.ShowBossSubjectMain(bossSubjectDetail);
			}
			break;
		}
		case EOperateType.WallSubject:
		{
			WallSubjectDetail wallSubjectDetail = zbSelf.Instance.wallSubjectHandler.GetWallSubjectDetail();
			if (wallSubjectDetail != null)
			{
				zbUIMgr.Instance.ShowWallSubject(wallSubjectDetail);
			}
			break;
		}
		case EOperateType.MechaSubject:
		{
			MechaSubjectDetail mechaSubjectDetail = zbSelf.Instance.mechaSubjectHandler.GetMechaSubjectDetail();
			if (mechaSubjectDetail != null)
			{
				zbUIMgr.Instance.ShowMechaSubject(mechaSubjectDetail);
			}
			break;
		}
		case EOperateType.DailySign:
			zbUIMgr.Instance.ShowDailySign();
			break;
		case EOperateType.TiliBank:
			zbUIMgr.Instance.ShowTiliBank();
			break;
		case EOperateType.LimitTimeVendor:
			if (zbSelf.Instance.holidayHandler.uuidLimitTimeVendor.Count > 0)
			{
				LimitTimeRouletteDetail limitTimeVendorInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeVendorInfoByUUID(zbSelf.Instance.holidayHandler.uuidLimitTimeVendor[0]);
				zbUIMgr.Instance.ShowLimitTimeRoulette(limitTimeVendorInfoByUUID);
			}
			else if (zbSelf.Instance.operateHandler.uuidLimitTimeVendor.Count > 0)
			{
				LimitTimeRouletteDetail limitTimeVendorInfoByUUID2 = zbSelf.Instance.operateHandler.GetLimitTimeVendorInfoByUUID(zbSelf.Instance.operateHandler.uuidLimitTimeVendor[0]);
				zbUIMgr.Instance.ShowLimitTimeRoulette(limitTimeVendorInfoByUUID2);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10759"));
			}
			break;
		case (EOperateType)30:
		case EOperateType.BubbleGift:
		case (EOperateType)34:
		case (EOperateType)35:
		case EOperateType.Community:
		case EOperateType.Discord:
		case (EOperateType)40:
		case (EOperateType)41:
		case (EOperateType)42:
		case (EOperateType)43:
		case (EOperateType)45:
			break;
		}
	}
}
