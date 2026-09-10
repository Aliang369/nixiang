using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.Accounts.Sdk;
using JetBrains.Annotations;
using UnityEngine;

public static class ThinkingAnalyticsExtends
{
	public static class SubPanelCatalog
	{
		public const string Common_SettingPanel = "通用-设置";

		public const string Common_AnnouncementPanel = "通用-公告";

		public const string Common_MonthCheckIn = "通用-每日签到";

		public const string Common_Mail = "通用-邮箱";

		public const string Common_GiftCode = "通用-礼包码";

		public const string Common_FestivalCheckIn = "通用-活动签到";

		public const string FortressLayer_StargazingRoom = "层级-观星室";

		public const string FortressLayer_IdolRoom = "层级-神像间";

		public const string FortressLayer_Library = "层级-档案馆";

		public const string FortressLayer_MainRoom = "层级-主仪式间";

		public const string StargazingRoom_Constellation = "观星室-星座界面";

		public const string StargazingRoom_Rank = "观星室-排行榜";

		public const string StargazingRoom_DifficultyRewardDetail = "观星室-难度奖励查看";

		public const string IdolRoom_LevelLine = "神像间-等级线";

		public const string IdolRoom_Idol = "神像间-魂祭界面";

		public const string IdolRoom_ForgetShrine = "神像间-遗忘界面";

		public const string Library_Index = "档案馆-图鉴";

		public const string Library_Index_Enemy = "档案馆-图鉴-敌人";

		public const string Library_Index_Enchant = "档案馆-图鉴-附魔";

		public const string Library_Index_Rune = "档案馆-图鉴-印记";

		public const string Library_Index_Influence = "档案馆-图鉴-祝福";

		public const string Library_Index_Card = "档案馆-图鉴-卡牌";

		public const string Library_Index_CardSkinSet = "档案馆-图鉴-卡牌形态";

		public const string Library_Index_Skill = "档案馆-图鉴-护符";

		public const string Library_Index_SkillGem = "档案馆-图鉴-护符宝石";

		public const string Library_GameRecordAndStatistics = "档案馆-游戏记录与统计数据";

		public const string Library_GameRecord_Detail = "档案馆-游戏记录详情";

		public const string Library_Achievement = "档案馆-成就";

		public const string Library_Forerunner = "档案馆-先行者遗物";

		public const string Library_Forerunner_Keep = "档案馆-先行者遗物-保管";

		public const string Library_Forerunner_Got = "档案馆-先行者遗物-收到";

		public const string Library_Forerunner_Leave = "档案馆-先行者遗物-留下";

		public const string Library_Forerunner_Legacy_NoteDetail = "档案馆-先行者遗物-留下";

		public const string Library_CardSkin_Detail = "档案馆-卡牌形态详情";

		public const string MainRoom_Storehouse = "主仪式间-仓库";

		public const string MainRoom_Storehouse_SkillGem = "主仪式间-仓库-护符宝石";

		public const string MainRoom_Storehouse_Skill = "主仪式间-仓库-护符";

		public const string MainRoom_Storehouse_Clavis = "主仪式间-仓库-钥匙";

		public const string MainRoom_Storehouse_Matter = "主仪式间-仓库-素材";

		public const string MainRoom_SkillConfig = "主仪式间-护符配置";

		public const string MainRoom_Character_Detail = "主仪式间-角色详情界面";

		public const string MainRoom_Character_Gathering = "主仪式间-角色选择界面";

		public const string MainRoom_CharacterDetail_Talent = "主仪式间-角色详情-天赋";

		public const string MainRoom_CharacterDetail_Obsession_Detail = "主仪式间-角色详情-执念等级";

		public const string MainRoom_GachaDraw = "主仪式间-探寻";

		public const string MainRoom_GachaDraw_Rule = "主仪式间-探寻规则";

		public const string MainRoom_NewbieFestival = "主仪式间-新生游艺";

		public const string MainRoom_YuanQiActivity = "主仪式间-元气活动";

		public const string MainRoom_Prepare_DiffucltySelect = "主仪式间-准备-难度选择";

		public const string MainRoom_Combine_Gem = "主仪式间-合成宝石";

		public const string MainRoom_Achievement = "主仪式间-成就";

		public const string MainRoom_Season = "主仪式间-战令界面";

		public const string MainRoom_Season_buy_level = "主仪式间-战令界面-购买等级";

		public const string MainRoom_DailyQuest = "主仪式间-每日任务界面";

		public const string Basement_UpgradeSkill = "地下室-强化护符";

		public const string Basement_UpgradeSkill_BreakThrough = "地下室-强化护符-护符突破";

		public const string Basement_UpgradeSkill_Add_Slot = "地下室-强化护符-护符打孔";

		public const string Basement_UpgradeSkill_Mount_Gem = "地下室-强化护符-护符镶嵌";

		public const string Basement_DeleteSkill = "地下室-销毁护符";

		public const string Basement_Purchase = "地下室-商店";

		public const string Basement_Purchase_SubGroup = "地下室-商店-子页面";

		public const string PVPMode_Main_Panel = "异步联机-PVP模式主界面";

		public const string PVPMode_Defender_Detail_Panel = "异步联机-守卫者详情界面";

		public const string PVPMode_Rank_Panel = "异步联机-守卫者排行榜界面";

		public const string PVPMode_Challenge_Game = "异步联机-挑战者准备游戏";

		public const string PVPMode_Storage_Insepct = "异步联机-组件仓库查看界面";

		public const string Other_Card_Recommand_Panel = "其他-卡组推荐界面";
	}

	public static class GetCardType
	{
		public const string BattleBonus = "战斗奖励";

		public const string BonusGame = "游艺";

		public const string Store = "商店";

		public const string Other = "其他";
	}

	public static class GetBlessType
	{
		public const string EliteBattle = "精英战斗";

		public const string CrystalUpgrade = "魂晶升级";

		public const string BossBless = "Boss祝福";

		public const string Other = "其他";
	}

	public enum ExceptionType
	{
		Other,
		UIException,
		Effect,
		Action,
		Coroutine,
		Text,
		SaveLoad,
		GameEvent,
		GlobalEvent,
		UnHandled
	}

	public static void TrackFortressMountSkillGem(ApplicationStateInfo applicationState, ProfileSkill skill, List<SkillGem> skillGems)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("skill_detail", TAFormatter.FormatSkillDetail(new GameSkill(skill))).Append("gem_list", TAFormatter.FormatSkillGemList(skillGems)).AppendApplicationCommonProperty(applicationState);
		TrackEvent("fortress_mount_skill_gem", properties);
	}

	public static void TrackFortressSkillAddGemSlot(ApplicationStateInfo applicationState, ProfileSkill skill, int newGemSlotCount, int oldGemSlotCount, bool isApplyAddSlot)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("skill_detail", TAFormatter.FormatSkillDetail(new GameSkill(skill))).Append("new_gem_slot_count", newGemSlotCount).Append("old_gem_slot_count", oldGemSlotCount)
			.Append("is_apply_add_slot", isApplyAddSlot)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("fortress_skill_add_gem_slot", properties);
	}

	public static void TrackFortressSkillBreakthrough(ApplicationStateInfo applicationState, ProfileSkill skill)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("skill_detail", TAFormatter.FormatSkillDetail(new GameSkill(skill))).AppendApplicationCommonProperty(applicationState);
		TrackEvent("fortress_skill_breakthrough", properties);
	}

	public static void TrackFortressCombineGem(ApplicationStateInfo applicationState, List<SkillGem> skillGems, SkillGem skillGem)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("gem_list", TAFormatter.FormatSkillGemList(skillGems)).Append("combine_new_gem", TAFormatter.FormatSkillGem(skillGem)).AppendApplicationCommonProperty(applicationState);
		TrackEvent("fortress_combine_gem", properties);
	}

	public static void TrackDeleteSkill(ApplicationStateInfo applicationStateInfo, List<ProfileSkill> deleteSkills)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("delete_skill_detail_list", TAFormatter.FormatSkillDetailList(deleteSkills)).Append("delete_skill_detail_list_length", deleteSkills.Count).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_delete_skill", properties);
	}

	public static void TrackChangeCharacterSkillSetup(ApplicationStateInfo applicationStateInfo, PresetSkillSlot slot, ProfileSkillPreset preset)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("character_id", preset.character.def.FullID).Append("new_skill", (slot.skill == null) ? null : TAFormatter.FormatSkillDetail(slot.skill)).Append("slot_index", slot.index)
			.Append("new_skill_detail_list", TAFormatter.FormatSkillDetailList(preset.AllConfigSkills))
			.Append("skill_preset_index", preset.index)
			.AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("foretress_change_character_skill_setup", properties);
	}

	public static void TrackUnlockCharacter(ApplicationStateInfo applicationStateInfo, PlayerCharacterDef character, string unlockMethod)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("character_id", character?.FullID).Append("character_unlock_method", unlockMethod).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_unlock_character", properties);
	}

	public static void TrackUnlockTalent(ApplicationStateInfo applicationState, TalentDef talent, PlayerCharacterState character)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("talent", talent.FullID).Append("talent_type", talent.talentType.ToString()).Append("character_id", character.def.FullID)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("fortress_unlock_talent", properties);
	}

	public static void TrackFortressUpgradeCharacterObsessionLevel(ApplicationStateInfo applicationState, PlayerCharacterState character, int newObsessionLevelIndex, MatterInfo matterInfo)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("character_id", character.def.FullID).Append("new_obsession_level_index", newObsessionLevelIndex).Append("upgrade_obsession_level_matter_id", matterInfo.matterDef.FullID)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("fortress_upgrade_character_obsession_level", properties);
	}

	public static void TrackFortressRecycleObsession(ApplicationStateInfo applicationState, List<MatterInfo> obsessions)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("recycle_obsession_list", TAFormatter.FormatMatterInfoList(obsessions)).AppendApplicationCommonProperty(applicationState);
		TrackEvent("fortress_recycle_obsession", properties);
	}

	public static void TrackIdolOfferSoul(ApplicationStateInfo applicationStateInfo, CharacterClassDef characterClass, int offerSoulLevel)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("character_class_id", characterClass?.FullID).Append("offer_soul_level", offerSoulLevel).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_idol_offer_soul", properties);
	}

	public static void TrackForgetShrineChange(ApplicationStateInfo applicationStateInfo, CharacterClassDef characterClass, string forgetId, string changeType)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("character_class_id", characterClass?.FullID).Append("forget_id", forgetId).Append("forget_change_type", changeType)
			.AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_forget_shrine_change", properties);
	}

	public static void TrackFortressGachaDraw(ApplicationStateInfo asi, int drawCount, GachaPoolDef pool, List<GlobalRewardInfo> rewardInfos, PlayerCharacterDef wishCharacter)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("gacha_draw_count", drawCount).Append("reward_info_list", TAFormatter.FormatGlobalRewardDetailList(rewardInfos)).Append("gacha_pool", pool.FullID)
			.Append("wish_target", wishCharacter?.FullID)
			.AppendApplicationCommonProperty(asi);
		TrackEvent("fortress_gacha_draw", properties);
	}

	public static void TrackNewbieFestivalDraw(ApplicationStateInfo asi, int poolCycleIndex, string poolDateId, string rewardTier, string drawTicketType, int remainingFreeDrawCount, int remainingPaidDrawCount, bool isPoolCompleted, GlobalRewardInfo rewardInfo)
	{
		List<GlobalRewardInfo> rewards = ((rewardInfo == null) ? new List<GlobalRewardInfo>() : new List<GlobalRewardInfo> { rewardInfo });
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "开服活动新生游艺202607").Append("draw_ticket_type", drawTicketType).Append("pool_cycle_index", poolCycleIndex)
			.Append("pool_date_id", poolDateId)
			.Append("reward_tier", rewardTier)
			.Append("reward_info_list", TAFormatter.FormatGlobalRewardDetailList(rewards))
			.Append("remaining_free_draw_count", remainingFreeDrawCount)
			.Append("remaining_paid_draw_count", remainingPaidDrawCount)
			.Append("is_pool_completed", isPoolCompleted)
			.AppendApplicationCommonProperty(asi);
		TrackEvent("activity_wheel_draw", properties);
	}

	public static void TrackActivityTaskAction(ApplicationStateInfo applicationState, string activityId, string taskId, string taskType, string actionType, List<GlobalRewardInfo> rewardInfos)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", activityId).Append("task_id", taskId).Append("task_type", taskType)
			.Append("action_type", actionType)
			.Append("reward_info_list", TAFormatter.FormatGlobalRewardDetailList(rewardInfos))
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_task_action", properties);
	}

	private static List<Dictionary<string, object>> FormatActivityTaskRewardInfoList(List<GlobalRewardInfo> rewardInfos)
	{
		return (from reward in rewardInfos ?? new List<GlobalRewardInfo>()
			where reward != null
			select new Dictionary<string, object>().Append("count", ((IGlobalItemDisplaySetup)reward).GlobalItemCount).Append("repeat", reward.giveAppendInfo?.repeated ?? false).Append("source_id", reward.rewardSourceId)
				.Append("source_type", reward.rewardSourceType.ToString())
				.Append("target_id", reward.RewardTargetId)
				.Append("type", GetActivityTaskRewardType(reward))).ToList();
	}

	private static string GetActivityTaskRewardType(GlobalRewardInfo reward)
	{
		if (reward?.matter?.matterDef == null)
		{
			return "item";
		}
		MatterDef matterDef = reward.matter.matterDef;
		if (matterDef.matterType == MatterDef.MatterType.GachaTicket || matterDef.matterType == MatterDef.MatterType.LimitGachaTicket)
		{
			return "ticket";
		}
		if (!matterDef.isCurrencyMatter)
		{
			return "item";
		}
		return "currency";
	}

	public static void TrackFortressGachaSetWish(ApplicationStateInfo asi, GachaPoolDef pool, PlayerCharacterDef wishCharacter)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("gacha_pool", pool.FullID).Append("character_id", wishCharacter?.FullID).AppendApplicationCommonProperty(asi);
		TrackEvent("fortress_gacha_set_wish", properties);
	}

	public static void TrackFinishPurchase(ApplicationStateInfo asi, PurchaseInfo purchase)
	{
		PurchaseGroupDef purchaseGroupDef = asi.purchaseManager.purchaseGroups.FirstOrDefault((PurchaseGroupDef g) => g.purchases.Contains(purchase));
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("purchase_id", purchase.purchaseId).Append("purchase_type", purchase.GetType().ToString()).Append("purchase_group", purchaseGroupDef?.GroupName)
			.AppendApplicationCommonProperty(asi);
		TrackEvent("fortress_finish_purchase", properties);
	}

	public static void TrackRealMoneyPurchaseProgress(ApplicationStateInfo asi, PurchaseInfo purchase, string progress, IReadOnlyDictionary<string, string> details = null)
	{
		PurchaseGroupDef purchaseGroupDef = asi.purchaseManager.purchaseGroups.FirstOrDefault((PurchaseGroupDef g) => g.purchases.Contains(purchase));
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("purchase_id", purchase.purchaseId).Append("purchase_type", purchase.GetType().ToString()).Append("purchase_group", purchaseGroupDef?.GroupName)
			.Append("purchase_progress", progress)
			.Append("purchase_error_details", FormatPurchaseFailureDetails(details))
			.AppendApplicationCommonProperty(asi);
		TrackEvent("fortress_real_money_purchase", properties);
	}

	private static List<string> FormatPurchaseFailureDetails(IReadOnlyDictionary<string, string> details)
	{
		if (details == null)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> detail in details)
		{
			string text = detail.Key ?? string.Empty;
			list.Add(text + ":" + (detail.Value ?? string.Empty));
		}
		return list;
	}

	public static void TrackFinishPromotionPopup(ApplicationStateInfo applicationStateInfo, string popupId, string result)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("promotion_popup_id", popupId).Append("promotion_interaction_result", result).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_finish_promotion_popup", properties);
	}

	public static void TrackEnterAdScene(ApplicationStateInfo applicationStateInfo, string adSceneName, bool isAdReady)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("ad_name", adSceneName).Append("is_ad_ready", isAdReady).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_enter_ad_scene", properties);
	}

	public static void TrackAdLoaded(ApplicationStateInfo applicationStateInfo, string adSceneName, int retryTimes, bool success)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("ad_name", adSceneName).Append("load_retry_times", retryTimes).Append("success", success)
			.AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_ad_load", properties);
	}

	public static void TrackShowAdScene(ApplicationStateInfo applicationStateInfo, string adSceneName)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("ad_name", adSceneName).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_show_ad_scene", properties);
	}

	public static void TrackAchievementFinish(ApplicationStateInfo applicationStateInfo, Achievement achievement)
	{
		int value = 0;
		if (!achievement.achievementDef.AchieveRewards.IsNullOrEmpty())
		{
			value = (achievement.achievementDef.AchieveRewards.OfType<MatterReward>()?.FirstOrDefault()?.amount).GetValueOrDefault();
		}
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("achievement_id", achievement.achievementDef.FullID).Append("achievement_matter_bonus", value).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("achievement_finish", properties);
	}

	public static void TrackRoutineQuestFinish(ApplicationStateInfo applicationStateInfo, RoutineQuest quest)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("achieved_quest", TAFormatter.FormatQuestDetail(quest)).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("routinequest_finish", properties);
	}

	public static void TrackFortressReplaceDailyQuest(ApplicationStateInfo applicationStateInfo, RoutineQuest oldQuest, RoutineQuest newQuest)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("quest_id_old", oldQuest.def.FullID).Append("quest_id_new", newQuest.def.FullID).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_replace_daily_quest", properties);
	}

	public static void TrackFestivalCheckIn(ApplicationStateInfo applicationStateInfo, FestivalCheckInResult festivalCheckInResult)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("festival_checkin_id", festivalCheckInResult.festivalDef.FullID).Append("checkin_days_count", festivalCheckInResult.alreadyCheckInCount).Append("reward_info", TAFormatter.FormatGlobalRewardDetail(festivalCheckInResult.reward))
			.AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_festival_checkin", properties);
	}

	public static void TrackMonthCheckIn(ApplicationStateInfo applicationStateInfo, MonthCheckInResult checkInResult)
	{
		applicationStateInfo.dailyCheckManager.monthCheckState.Count((MonthCheckInState m) => m.isCurrentMonth && m.isCollected);
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("checkin_days_count", checkInResult.checkInDaysThisMonth.Count).Append("checkin_day_index", DateTime.Now.Day).Append("reward_info", TAFormatter.FormatGlobalRewardDetail(checkInResult.reward))
			.AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_month_checkin", properties);
	}

	public static void TrackAcceptMail(ApplicationStateInfo asi, MailData mail)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("mail_guid", mail.id.ToString()).AppendApplicationCommonProperty(asi);
		TrackEvent("fortress_accept_mail", properties);
	}

	public static void TrackLevelLineChangeExp(ApplicationStateInfo applicationStateInfo, LevelLine.ChangeExpResult changeExp)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("add_exp", changeExp.request.deltaExp).Append("initLevel", changeExp.initLeveLIndex).Append("finalLevel", changeExp.finalLevelIndex)
			.Append("levelline_state", TAFormatter.FormatLevelLineState(changeExp.GenerateAfterLevelLine()))
			.Append("reward_info_list", TAFormatter.FormatGlobalRewardDetailList(changeExp.GetAllUnFoldRewards()))
			.AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_level_line_change_exp", properties);
	}

	public static void TrackFortressManualAddLevelLineLevel(ApplicationStateInfo asi, LevelLine levelline, int initLevel, int addLevel)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("levelline_state", TAFormatter.FormatLevelLine(levelline)).Append("init_level", initLevel).Append("add_level", addLevel)
			.AppendApplicationCommonProperty(asi);
		TrackEvent("fortress_manual_add_level_line_level", properties);
	}

	public static void TrackFortressManualGetLevelLineReward(ApplicationStateInfo asi, LevelLine levelline, List<LevelLineDef.LineLevel> lineLevels)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("levelline_state", TAFormatter.FormatLevelLine(levelline)).Append("manual_get_reward_level_list", TAFormatter.FormatLineLevelList(lineLevels, levelline)).AppendApplicationCommonProperty(asi);
		TrackEvent("fortress_manual_get_level_line_reward", properties);
	}

	public static void TrackGetIndexbookReward(ApplicationStateInfo applicationStateInfo, string rewardType, string pageType, int rewardCount, int rewardLevel, float exploreProgress)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("index_reward_type", rewardType).Append("index_page_type", pageType).Append("index_reward_count", rewardCount)
			.Append("index_reward_level", rewardLevel)
			.Append("explore_progress", exploreProgress)
			.AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_get_indexbook_reward", properties);
	}

	public static void TrackFinishStoryPoint(ApplicationStateInfo applicationStateInfo, StoryPoint storyPoint)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("storypoint_id", storyPoint.FullID).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("finish_storypoint", properties);
	}

	public static void TrackEnterCutscene(ApplicationStateInfo applicationStateInfo, string cutsceneFullId)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("cutscene_id", cutsceneFullId).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_enter_cutscene", properties);
	}

	public static void TrackFinishCutscene(ApplicationStateInfo applicationStateInfo, string cutsceneFullId, bool skip)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("cutscene_id", cutsceneFullId).Append("cutscene_skipped", skip).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_finish_cutscene", properties);
	}

	public static void TrackTalkToFortressNPC(ApplicationStateInfo applicationStateInfo, FortressNPC npc)
	{
		FortressNPC.ProgressNode currentProgressNode = npc.currentProgressNode;
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("fortress_npc_id", npc.FullId).Append("fortress_npc_progress", currentProgressNode.activateStoryPoint.FullID).Append("fortress_npc_first_talk", currentProgressNode.IsFinished(npc, applicationStateInfo))
			.AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_talk_to_fortress_npc", properties);
	}

	public static void TrackFortressProgressNode(ApplicationStateInfo applicationState, string node)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("progress_node", node).AppendApplicationCommonProperty(applicationState);
		TrackEvent("fortress_progress_node", properties);
	}

	public static void TrackMatterChange(ApplicationStateInfo applicationState, MatterDef matter, int delta, int value, string cause)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("matter_id", matter?.FullID).Append("delta", delta).Append("value", value)
			.Append("cause", cause)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("matter_change", properties);
	}

	public static void TrackFortressOpenLootMatter(ApplicationStateInfo applicationStateInfo, MatterDef matterDef, int lootSelectIndex)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("matter_id", matterDef.FullID).Append("loot_select_index", lootSelectIndex).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_open_loot_matter", properties);
	}

	public static void TrackGetGlobalRewardInfo(ApplicationStateInfo applicationStateInfo, GlobalRewardInfo reward)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("reward_info", TAFormatter.FormatGlobalRewardDetail(reward)).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_get_global_reward_info", properties);
	}

	public static void TrackFortressPvpUploadDefender(ApplicationStateInfo applicationStateInfo, PVPDefender defender)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("pvp_defender", TAFormatter.FormatDefender(defender)).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_pvp_upload_defender", properties);
	}

	public static void TrackFortressPvpRefreshCompStorage(ApplicationStateInfo applicationStateInfo, List<DeckPresetComp> comps)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("new_pvp_comp_list", TAFormatter.FormatDeckPresetCompList(comps)).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_pvp_refresh_comp_storage", properties);
	}

	public static void TrackStopKeepForerunnerLegacy(ApplicationStateInfo asi, ForerunnerLegacy legacy)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("forerunner_type", legacy.shareLegacy ? "分享" : "本地").Append("forerunner_guid", legacy.guid).Append("forerunner_note", legacy.note)
			.Append("forerunner_card", (legacy.legacyItem == null) ? null : TAFormatter.FormatItemDetail(legacy.legacyItem.DisplayItem))
			.Append("forerunner_skill", (legacy.legacySkill == null) ? null : TAFormatter.FormatSkillDetail(legacy.legacySkill.DisplaySkill))
			.AppendApplicationCommonProperty(asi);
		TrackEvent("fortress_stop_keep_forerunner_legacy", properties);
	}

	public static void TrackGameStart(GameStateInfo gameState, bool newGame, bool guide)
	{
		string value = ((!newGame) ? "载入游戏" : (guide ? "新手引导" : "开始新游戏"));
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("start_times", gameState.applicationState.gameStatisticsManager.startNewGameTimesSum).Append("gamestart_type", value).Append("skill_setup_detail", TAFormatter.FormatSkillDetailList(gameState.playerState.AllSkills))
			.Append("skill_setup_detail_length", gameState.playerState.AllSkills.Count())
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_gamestart", properties);
	}

	public static void TrackGameOver(GameStateInfo gameState, GameRecord record)
	{
		List<GlobalRewardInfo> allRewardInfos = record.GetAllRewardInfos();
		List<LevelLine> allLevelLine = LevelLineDef.GetAllLevelLine(gameState.applicationState);
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("deck_detail", TAFormatter.FormatItemDetailList(record.deck.ConvertAll((GameRecord.ItemRecord info) => info.DisplayItem))).Append("bless_list", TAFormatter.FormatInfluenceList(record.influences.ConvertAll((GameRecord.InfluenceRecord info) => info.DisplayInfluence))).Append("skill_list_detail", TAFormatter.FormatSkillDetailList(record.skillInfos.ConvertAll((GameRecord.SkillRecord s) => s.DisplaySkill)))
			.Append("deck_sum_rune_list", TAFormatter.FormatRuneList(record.runes.ConvertAll((GameRecord.RuneRecord r) => r.DisplayRune)))
			.Append("gameover_type", (!record.finished) ? "放弃" : (record.success ? "胜利" : "死亡"))
			.Append("sum_coin", gameState.recordManager.sumGetCoin)
			.Append("sum_crystal", gameState.recordManager.sumGetCrystal)
			.Append("exp", record.exp)
			.Append("gameover_reward_info", TAFormatter.FormatGlobalRewardDetailList(allRewardInfos))
			.Append("levelline_state_list", TAFormatter.FormatLevelLineStateList(allLevelLine))
			.Append("rank_score", record.rankScore)
			.Append("activated_potential_list", (from r in record.gameJournal.records.OfType<ActivatePotentialTalentRecord>()
				select r.talentId).ToList())
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_gameover", properties);
	}

	public static void TrackBattleOver(GameStateInfo gameState, FinishBattleEncounterAction.FinishType finishType)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("enemy", gameState.currentEncounter.enemyAI.component.FullId).Append("battle_time", new TimeSpan(gameState.gameTimer.GetTime() - gameState.recordManager.battleInitTimeTick).TotalSeconds).Append("enemy_type", gameState.currentEncounter.enemyAI.enemyEncounterType.ToString())
			.Append("health_lost", gameState.recordManager.battleInitPlayerHealth - (int)gameState.playerState.bft.health)
			.Append("battleover_type", finishType.ToString())
			.Append("room", gameState.currentEncounter.room.FullId)
			.Append("fps", gameState.currentEncounter.room.GetFPS())
			.Append("battle_guid", gameState.battleState.battleGUID)
			.Append("turn_count", gameState.battleState.turnCount)
			.Append("environment_influence", gameState.currentEncounter.environmentInfluence?.def.FullID)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_battleover", properties);
	}

	public static void TrackRoundOver(GameStateInfo gameState, bool battleStop)
	{
		List<Damage> source = gameState.recordManager.damageThisRound.FindAll((Damage d) => d.targetBFT != null && d.targetBFT.isPlayer && d.isHealthDamage);
		List<Damage> source2 = gameState.recordManager.damageThisRound.FindAll((Damage d) => d.targetBFT != null && !d.targetBFT.isPlayer && d.isHealthDamage);
		List<Damage> list = gameState.recordManager.damageThisRound.Where((Damage d) => d.isHealthDamage).ToList();
		List<Item> playerCastedItemsThisRound = gameState.recordManager.playerCastedItemsThisRound;
		List<GameSkill> castedSkillsThisRound = gameState.recordManager.castedSkillsThisRound;
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("battle_guid", gameState.battleState.battleGUID).Append("damage_to_player", source.Sum((Damage d) => d.subCounter)).Append("damage_to_enemy", source2.Sum((Damage d) => d.subCounter))
			.Append("cast_card_list", TAFormatter.FormatItemList(playerCastedItemsThisRound))
			.Append("cast_card_list_length", playerCastedItemsThisRound.Count)
			.Append("cast_skill_list", TAFormatter.FormatSkillList(castedSkillsThisRound))
			.Append("cast_skill_list_length", castedSkillsThisRound.Count)
			.Append("turn_count", gameState.battleState.turnCount)
			.Append("round_over_type", battleStop ? "战斗结束" : "新回合")
			.Append("round_time", new TimeSpan(gameState.gameTimer.GetTime() - gameState.recordManager.roundStartTimeTick).TotalSeconds)
			.Append("overflow_card_list", TAFormatter.FormatItemList(gameState.recordManager.overflowConsumeItemsThisRound))
			.Append("table_card_list_length", gameState.playerState.TableContainer.items.Count)
			.Append("overflow_card_list_length", gameState.recordManager.overflowConsumeItemsThisRound.Count)
			.Append("table_card_list", TAFormatter.FormatItemList(gameState.playerState.TableContainer.items))
			.Append("round_damage_detail", TAFormatter.FormatDamageDetailList(list))
			.Append("round_damage_to_enemy_sum", list.Where((Damage d) => d.targetBFT?.isEnemy ?? false).Sum((Damage d) => d.subCounter))
			.Append("round_damage_to_enemy_times", list.Count((Damage d) => d.targetBFT?.isEnemy ?? false))
			.Append("round_damage_to_player_sum", list.Where((Damage d) => d.targetBFT?.isPlayer ?? false).Sum((Damage d) => d.subCounter))
			.Append("round_damage_to_player_times", list.Count((Damage d) => d.targetBFT?.isPlayer ?? false))
			.Append("enemy", gameState.battleState.enemy?.component.FullId)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_round_over", properties);
	}

	public static void TrackWorldEvent(GameStateInfo gameState, WorldEvent worldEvent)
	{
		double totalSeconds = new TimeSpan(gameState.gameTimer.GetTime() - worldEvent.initTime).TotalSeconds;
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("event", worldEvent.FullID).Append("event_choice", worldEvent.lastSelectChoiceIndex).Append("time_cost", totalSeconds)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_worldevent", properties);
	}

	public static void TrackExposeBonusCard(GameStateInfo gameState, Item item, string type, bool selectable)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("card", item.def.FullID).Append("expose_type", type).Append("expose_selectable", selectable)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_bonus_expose_card", properties);
	}

	public static void TrackBonusCard(GameStateInfo gameState, Item item, string type)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("card", item.def.FullID).Append("expose_type", type).Append("card_quality", item.qualityLevel.ToString())
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_bonus_card", properties);
	}

	public static void TrackExposeBonusBless(GameStateInfo gameState, InfluenceDef influence, string type, bool selectable)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("bless", influence.FullID).Append("expose_type", type).Append("expose_selectable", selectable)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_bonus_expose_bless", properties);
	}

	public static void TrackBonusBless(GameStateInfo gameState, InfluenceDef influence, string type)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("bless", influence.FullID).Append("expose_type", type).AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_bonus_bless", properties);
	}

	public static void TrackExposeBonusRune(GameStateInfo gameState, Rune rune, MintageRunePanel_Event mintageEvent)
	{
		string text = "印记地点自带";
		if (mintageEvent.rerolled)
		{
			text += "刷新重Roll";
		}
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("rune", rune.def.FullID).Append("expose_type", text).Append("expose_selectable", value: true)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_bonus_expose_rune", properties);
	}

	public static void TrackBonusRune(GameStateInfo gameState, Rune rune, Item card, MintageRunePanel_Event mintageEvent)
	{
		string text = "印记地点";
		if (mintageEvent.rerolled)
		{
			text += "刷新";
		}
		if (mintageEvent.forceMintaged)
		{
			text += "强制";
		}
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("rune", rune.def.FullID).Append("card", card.def.FullID).Append("expose_type", text)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_bonus_rune", properties);
	}

	public static void TrackMintageRuneEvent(GameStateInfo gameState, MintageRunePanel_Event mintageEventPanel)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("mintage_rune_list", TAFormatter.FormatRuneList(mintageEventPanel.attachRunes)).Append("card", mintageEventPanel.attachItem?.def.FullID ?? "").Append("is_refresh", mintageEventPanel.rerolled)
			.Append("is_attach", mintageEventPanel.mintaged)
			.Append("is_force_mintage", mintageEventPanel.forceMintaged)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_mintage_rune", properties);
	}

	public static void TrackCrystalUpgrade(GameStateInfo gameState, CrystalUpgrade upgrade = null)
	{
		string value = "未知";
		string value2 = "";
		if (upgrade is GetCrystalUpgrade)
		{
			value = "获取魂晶离开";
		}
		if (upgrade is BlessCrystalUpgrade blessCrystalUpgrade)
		{
			value = "获取祝福";
			value2 = blessCrystalUpgrade.def.FullID;
		}
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("selection", value).Append("get_bless", value2).Append("consume_crystal", upgrade?.GetFinalPrice(gameState, real: false) ?? 0)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_crystal_upgrade", properties);
	}

	public static void TrackHealEvent(GameStateInfo gameState, HealSpringEvent healEvent, TalentDef activateTalent, GameSkill activateSkill, int healedAmount)
	{
		string value = "";
		string value2 = "离开";
		if (activateTalent != null)
		{
			value2 = "激活潜能";
			value = activateTalent.FullID;
		}
		else if (activateSkill != null)
		{
			value2 = "激活护符";
			value = activateSkill.skillDef.FullID;
		}
		else if (healedAmount != 0)
		{
			value2 = "恢复生命";
			value = healedAmount.ToString();
		}
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("heal_location_choice_type", value2).Append("heal_location_choice_detail", value).Append("heal_location_heal_amount", healedAmount)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_heal_location", properties);
	}

	public static void TrackStore(GameStateInfo gameState, StoreEvent storeEvent)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("buy_card_list", TAFormatter.FormatItemList(storeEvent.boughtItemsFromStore.ConvertAll((StoreEvent.PricedItem pi) => pi.item))).Append("sell_card_list", TAFormatter.FormatItemList(storeEvent.soldItems.ConvertAll((StoreEvent.PricedItem pi) => pi.item))).Append("buy_card_llist_length", storeEvent.boughtItemsFromStore.ConvertAll((StoreEvent.PricedItem pi) => pi.item).Count)
			.Append("sell_card_list_length", storeEvent.soldItems.ConvertAll((StoreEvent.PricedItem pi) => pi.item).Count)
			.Append("is_refresh", storeEvent.refreshTimes > 0)
			.Append("coin_cost", storeEvent.initCoin - (int)gameState.playerState.coinCount)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_store", properties);
	}

	public static void TrackUpgrade(GameStateInfo gameState, UpgradeRoomEvent upgradeRoomEvent)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("upgrade_card_list", TAFormatter.FormatItemList(gameState.UIManager.worldEventPanel_UpgradeRoom.upgradePanel.upgradedItems)).Append("upgrade_card_list_length", gameState.UIManager.worldEventPanel_UpgradeRoom.upgradePanel.upgradedItems.Count).Append("crystal_cost", gameState.UIManager.worldEventPanel_UpgradeRoom.upgradePanel.initCrystal - (int)gameState.playerState.crystalCount)
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_upgrade_room", properties);
	}

	public static void TrackActivateSkill(GameStateInfo gameState, GameSkill skill)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("skill", skill.skillDef.FullID).AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_activate_skill", properties);
	}

	public static void TrackExposeLocation(GameStateInfo gameState, PathLocation location)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().AppendLocationCommonProperty(location).AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_location_expose", properties);
	}

	public static void TrackEnterLocation(GameStateInfo gameState, PathLocation location)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().AppendLocationCommonProperty(location).AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_location_enter", properties);
	}

	public static void TrackExitLocation(GameStateInfo gameState, PathLocation location)
	{
		if (location != null)
		{
			string value = "";
			if (location.attachEncounterSetupInfo?.fixedEnemyInfoData != null)
			{
				value = location.attachEncounterSetupInfo.fixedEnemyInfoData.FullId;
			}
			else if (location.attachEncounterSetupInfo?.fixedEventTemplate != null)
			{
				value = location.attachEncounterSetupInfo.fixedEventTemplate.FullID;
			}
			Dictionary<string, object> properties = new Dictionary<string, object>().Append("location_time", new TimeSpan(gameState.gameTimer.GetTime() - gameState.recordManager.locationInitTimeTick).TotalSeconds).Append("location_encounter", value).AppendLocationCommonProperty(location)
				.AppendGameplayCommonProperty(gameState);
			TrackEvent("gameplay_location_exit", properties);
		}
	}

	public static void TrackForerunnerDeath(GameStateInfo gameState, [CanBeNull] ForerunnerLegacy legacy, bool share)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("forerunner_guid", legacy?.guid).Append("forerunner_legacy_is_leave", legacy != null).Append("forerunner_legacy_is_share", share)
			.Append("forerunner_note", legacy?.note)
			.Append("forerunner_card", (legacy?.legacyItem == null) ? null : TAFormatter.FormatItemDetail(legacy.legacyItem.DisplayItem))
			.Append("forerunner_skill", (legacy?.legacySkill == null) ? null : TAFormatter.FormatSkillDetail(legacy.legacySkill.DisplaySkill))
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_forerunner_death", properties);
	}

	public static void TrackActivatePotential(GameStateInfo gameState, TalentDef potential)
	{
		List<TalentDef> currentActivateAblePotentialTalent = gameState.playerState.CurrentActivateAblePotentialTalent;
		currentActivateAblePotentialTalent.AddUnique(potential);
		PlayerCharacterState playerCharacterState = gameState.applicationState.characterManager.GetAllUnlockedCharacterStates().FirstOrDefault((PlayerCharacterState c) => c.def.sharePotential == potential);
		bool value = playerCharacterState != null && gameState.playerState.character != playerCharacterState;
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("potential_id", potential.FullID).Append("is_share_potential", value).Append("activatable_potential_list", currentActivateAblePotentialTalent.ConvertAll((TalentDef t) => t.FullID))
			.AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_activate_potential", properties);
	}

	public static void TrackActivateStartGameBuff(GameStateInfo gameState, List<StartGameBuff> buffs)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activate_startgamebuff_list", buffs.ConvertAll((StartGameBuff b) => b.def.FullID)).AppendGameplayCommonProperty(gameState);
		TrackEvent("gameplay_activate_startgamebuff", properties);
	}

	public static void TrackFinishForerunnerEvent(GameStateInfo gs, ForerunnerWorldEvent forerunnerWorldEvent)
	{
		ForerunnerLegacy currentLegacy = forerunnerWorldEvent.currentLegacy;
		StoryNoteFeedback feedback = forerunnerWorldEvent.feedback;
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("forerunner_type", currentLegacy.shareLegacy ? "分享" : "本地").Append("forerunner_guid", currentLegacy.guid).Append("forerunner_note", currentLegacy.note)
			.Append("forerunner_card", (currentLegacy.legacyItem == null) ? null : TAFormatter.FormatItemDetail(currentLegacy.legacyItem.DisplayItem))
			.Append("forerunner_skill", (currentLegacy.legacySkill == null) ? null : TAFormatter.FormatSkillDetail(currentLegacy.legacySkill.DisplaySkill))
			.Append("forerunner_vote_result", (feedback.upvote == true) ? "点赞" : ((feedback.upvote == false) ? "点踩" : "未投票"))
			.Append("forerunner_take", feedback.agree)
			.AppendGameplayCommonProperty(gs);
		TrackEvent("gameplay_finsh_foreruuner", properties);
	}

	public static void TrackAfterBossUpgrade(GameStateInfo gs, AfterBossUpgrade upgrade)
	{
		BossBlessUpgrade bossBlessUpgrade = upgrade as BossBlessUpgrade;
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("bless", bossBlessUpgrade?.inf.def.FullID).Append("consume_willpower", bossBlessUpgrade?.WillPowerCost ?? 0).AppendGameplayCommonProperty(gs);
		TrackEvent("gameplay_after_boss_upgrade", properties);
	}

	public static void TrackSelectNextStageLevel(GameStateInfo gs, List<DraftMapRef> exposedMaps, DraftMapRef selectedMap)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("selected_next_map", selectedMap?.CTAssetId).Append("exposed_all_map", exposedMaps.ConvertAll((DraftMapRef m) => m.CTAssetId)).AppendGameplayCommonProperty(gs);
		TrackEvent("gameplay_select_next_stage_level", properties);
	}

	public static void TrackSkinUpgrade(ApplicationStateInfo applicationStateInfo, ItemDefinition card, ItemSkinDef skin, string upgradeType)
	{
		int value = skin.SetBatchConfig?.GetUnlockedCount(applicationStateInfo) ?? 0;
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("card", card?.FullID).Append("card_skin", skin?.FullID).Append("collect_set_count", value)
			.Append("skin_upgrade_type", upgradeType)
			.AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("gameplay_skin_upgrade", properties);
	}

	public static void TrackIntermissionPlayEntranceExpose(ApplicationStateInfo applicationState, int dayIndex, bool hasRedPoint)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "#ACT#000003#").Append("day_index", dayIndex).Append("has_red_point", hasRedPoint)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_intermission_play_entrance_expose", properties);
	}

	public static void TrackIntermissionPlayEntranceClick(ApplicationStateInfo applicationState, int dayIndex)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "#ACT#000003#").Append("day_index", dayIndex).AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_intermission_play_entrance_click", properties);
	}

	public static void TrackIntermissionPlayOpen(ApplicationStateInfo applicationState, int dayIndex)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "#ACT#000003#").Append("day_index", dayIndex).AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_intermission_play_open", properties);
	}

	public static void TrackIntermissionPlayDrawClick(ApplicationStateInfo applicationState, int dayIndex, string requestId, int cardIndex)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "#ACT#000003#").Append("day_index", dayIndex).Append("request_id", requestId)
			.Append("card_index", cardIndex)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_intermission_play_draw_click", properties);
	}

	public static void TrackIntermissionPlayDrawResult(ApplicationStateInfo applicationState, int dayIndex, string requestId, int cardIndex, int slotId, string ticketType, GlobalRewardInfo reward, int remainingSlotCount)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "#ACT#000003#").Append("day_index", dayIndex).Append("request_id", requestId)
			.Append("card_index", cardIndex)
			.Append("slot_id", slotId)
			.Append("ticket_type", ticketType)
			.Append("reward_info", TAFormatter.FormatGlobalRewardDetail(reward))
			.Append("remaining_slot_count", remainingSlotCount)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_intermission_play_draw_result", properties);
	}

	public static void TrackIntermissionPlayDrawFailure(ApplicationStateInfo applicationState, int dayIndex, string requestId, int cardIndex, string errorCode)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "#ACT#000003#").Append("day_index", dayIndex).Append("request_id", requestId)
			.Append("card_index", cardIndex)
			.Append("error_code", errorCode)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_intermission_play_draw_failure", properties);
	}

	public static void TrackIntermissionPlayReclaimConfirm(ApplicationStateInfo applicationState, int dayIndex, string requestId, string ticketType, int ticketCount, int giftCount)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "#ACT#000003#").Append("day_index", dayIndex).Append("request_id", requestId)
			.Append("ticket_type", ticketType)
			.Append("ticket_count", ticketCount)
			.Append("gift_count", giftCount)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_intermission_play_reclaim_confirm", properties);
	}

	public static void TrackIntermissionPlayReclaimResult(ApplicationStateInfo applicationState, int dayIndex, string requestId, string ticketType, int ticketCount, int giftCount)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "#ACT#000003#").Append("day_index", dayIndex).Append("request_id", requestId)
			.Append("ticket_type", ticketType)
			.Append("ticket_count", ticketCount)
			.Append("gift_count", giftCount)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_intermission_play_reclaim_result", properties);
	}

	public static void TrackIntermissionPlayReclaimFailure(ApplicationStateInfo applicationState, int dayIndex, string requestId, string ticketType, int ticketCount, string errorCode)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "#ACT#000003#").Append("day_index", dayIndex).Append("request_id", requestId)
			.Append("ticket_type", ticketType)
			.Append("ticket_count", ticketCount)
			.Append("error_code", errorCode)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_intermission_play_reclaim_failure", properties);
	}

	public static void TrackIntermissionPlayPoolEmpty(ApplicationStateInfo applicationState, int dayIndex, string requestId)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("activity_id", "#ACT#000003#").Append("day_index", dayIndex).Append("request_id", requestId)
			.AppendApplicationCommonProperty(applicationState);
		TrackEvent("activity_intermission_play_pool_empty", properties);
	}

	public static void TrackAppActive(long activeTimes)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("first_active", activeTimes == 0).Append("active_times", activeTimes);
		TrackEvent("app_on_active", properties);
	}

	public static void TrackLoginFirstSuccess()
	{
		TrackEvent("first_login", new Dictionary<string, object>());
	}

	public static void TrackSignUp()
	{
		TrackEvent("sign_up", new Dictionary<string, object>());
	}

	public static void TrackLoginSuccess(long activeTimes)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("first_active", activeTimes == 0);
		TrackEvent("app_login_success", properties);
	}

	public static void TrackEnterLoginMenu()
	{
		Dictionary<string, object> properties = new Dictionary<string, object>();
		TrackEvent("enter_login_menu", properties);
	}

	public static void TrackAccountBinding(int bindingType)
	{
		string value = null;
		switch (bindingType)
		{
		case 1:
			value = "发起账号绑定";
			break;
		case 2:
			value = "账号绑定失败";
			break;
		case 3:
			value = "账号绑定成功";
			break;
		}
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("binding_type", value);
		TrackEvent("account_binding", properties);
	}

	public static void TrackApplicationStateContext(ApplicationStateInfo applicationState, string gameType)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("application_state_context", gameType).Append("game_guid", applicationState?.runningGameState?.GameGUID).AppendApplicationCommonProperty(applicationState);
		TrackEvent("after_load_application_state", properties);
	}

	public static void TrackDropLastSaveStartGame(bool isDropGame)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("is_drop_current_game", isDropGame);
		TrackEvent("drop_last_game_save_start", properties);
	}

	public static void TrackSettingChange(string type, string value, ApplicationStateInfo applicationStateInfo = null)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("settings_type", type).Append("settings_value", value);
		if (applicationStateInfo != null)
		{
			properties.AppendApplicationCommonProperty(applicationStateInfo);
		}
		TrackEvent("settings_change", properties);
	}

	public static void TrackOpenFortressSubPanel(ApplicationStateInfo applicationStateInfo, string panelId, string panelAppendInfo = null)
	{
		Dictionary<string, object> properties = new Dictionary<string, object>().Append("panel_id", panelId).Append("open_panel_append_info", panelAppendInfo).AppendApplicationCommonProperty(applicationStateInfo);
		TrackEvent("fortress_open_subpanel", properties);
	}

	public static void Init()
	{
		TAAdapter.Init();
	}

	public static void LoginTA(string playerId)
	{
		TAAdapter.Login(playerId);
	}

	public static string GetDistinctId()
	{
		return TAAdapter.GetDistinctId();
	}

	public static void SetUser(ApplicationStateInfo applicationState)
	{
		TAAdapter.UserSet(new Dictionary<string, object>().Append("unique_id", TAAdapter.GetDeviceId()).Append("user_id", AccountStateManager.Instance?.State?.User?.Id.ToString()).Append("version_name", Application.version)
			.Append("version_name2", AppVersionDisplay.GetDisplayVersion())
			.Append("game_time", new TimeSpan(applicationState.playTimeRecordManager.GetPlayTime()).TotalSeconds)
			.Append("token_count", ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter.GetCount(applicationState))
			.Append("currency_count", ScriptableObjectSingleton<FortressDataManager>.instance.commonCurrencyMatter.GetCount(applicationState))
			.Append("achieved_achievement", applicationState.achievementManager.GetAllAchievedAchievements().ConvertAll((Achievement a) => a.achievementDef.FullID))
			.Append("achieved_missions", TAFormatter.FormatRoutineQuestList(applicationState.routineQuestManager.currentWeekQuests))
			.Append("storypoint_finished", (from s in applicationState.assetCatalog.storyPointCatalog.ValidAssets
				where s.IsFinished(applicationState)
				select s.FullID).ToList())
			.Append("global_level", applicationState.globalUnlockManager.globalLevelLine.currentLevelIndex)
			.Append("clavis_unlocked", (from t in applicationState.assetCatalog.clavisDefDefCatalog.ValidAssets
				where applicationState.clavisManager.IsUnlocked(t)
				select t.FullID).ToList())
			.Append("skill_pool", TAFormatter.FormatSkillList(applicationState.globalSkillManager.allSkills))
			.Append("skill_detail_pool", TAFormatter.FormatSkillDetailList(applicationState.globalSkillManager.allSkills))
			.Append("skill_pool_count", applicationState.globalSkillManager.allSkills.Count)
			.Append("unlock_character_list", (from c in applicationState.characterManager.GetAllUnlockedCharacterStates()
				select c.def.FullID).ToList())
			.Append("character_pool_count", applicationState.characterManager.GetAllUnlockedCharacterStates().Count())
			.Append("essence_currency_count", ScriptableObjectSingleton<FortressDataManager>.instance.essenceCurrencyMatter.GetCount(applicationState))
			.Append("credit_currency_count", ScriptableObjectSingleton<FortressDataManager>.instance.creditCurrencyMatter.GetCount(applicationState)));
	}

	public static void TraceException(string message, string stackTrace, ExceptionType type)
	{
		TAAdapter.ReportException("【" + type.ToString() + "】" + message, message, stackTrace);
	}

	private static void TrackEvent(string eventName, Dictionary<string, object> properties)
	{
		properties.AppendGlobalCommonProperty();
		try
		{
			TAAdapter.Track(eventName, properties);
		}
		catch (Exception arg)
		{
			TraceLog.Error($"[TA] Track failed: {arg}");
		}
	}
}
