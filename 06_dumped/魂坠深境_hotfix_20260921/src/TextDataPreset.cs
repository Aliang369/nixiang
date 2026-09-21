using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ContainerOfSoul.Display.Fortress.GameConfig;
using OdinEx;
using UnityEngine;

public class TextDataPreset : ScriptableObjectSingleton<TextDataPreset>
{
	public class ForceUpdateManagerText
	{
		[LocalizeText("零碎文本")]
		[TextArea(1, 2)]
		public string term_found_new_version_restart_update = "发现新版本，请重启更新";

		[LocalizeText("零碎文本")]
		[TextArea(1, 2)]
		public string term_maintenance_update_until = "到{0}前为停服更新时间，请稍后登录";

		[LocalizeText("零碎文本")]
		[TextArea(1, 2)]
		public string term_maintenance_update_preview = "{0}为停服更新，届时将会强制退出登录";

		[LocalizeText("零碎文本")]
		[TextArea(1, 2)]
		public string term_unavailable_version_info = "无法拉取到版本信息";

		[LocalizeText("零碎文本")]
		[TextArea(1, 2)]
		public string term_exit = "退出";

		[LocalizeText("零碎文本")]
		public string term_close = "关闭";
	}

	public class GameAsyncOperationText
	{
		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_network_communicating = "通信中……";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_player_data = "正在加载玩家数据";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_gacha_history = "正在加载抽卡记录";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_activity_config = "正在加载活动配置";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_syncing_gacha_history = "正在同步抽卡记录";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_forerunner_data = "正在加载先行者数据";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_updating_forerunner_legacy = "正在更新先行者遗物";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_unlocking_character = "正在解锁角色";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_uploading_achievement = "正在同步成就";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_activating_start_game_buff = "正在激活开局增益";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_checking_data_migration = "正在检查账号数据";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_migrating_data = "正在迁移账号数据";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_daily_check_in = "正在进行每日签到";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_unlocking_clavis = "正在解锁钥石";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_activating_clavis = "正在激活钥石";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_yuanqi_activity = "正在加载元气活动";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_claiming_yuanqi_activity_reward = "正在领取元气活动奖励";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_syncing_yuanqi_activity_progress = "正在同步元气活动进度";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_character_trial = "正在加载角色试炼";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_submitting_review_feedback = "正在提交评价反馈";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_opening_questionnaire = "正在打开问卷";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_breaking_through_skill = "正在突破护符";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_syncing_favorite_skills = "正在同步收藏护符";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_setting_gacha_wish = "正在设置祈愿目标";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_gacha_draw = "正在进行抽卡";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_fetching_index_reward = "正在领取图鉴奖励";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_generating_guide_progress = "正在同步引导进度";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_updating_skill_gem_slots = "正在扩展宝石孔位";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_mounting_skill_gems = "正在保存宝石镶嵌";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_combining_skill_gems = "正在合成宝石";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_gift_code_config = "正在加载礼包码配置";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_redeeming_gift_code = "正在兑换礼包码";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_claiming_quest_reward = "正在领取任务奖励";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_refreshing_quest = "正在刷新任务";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_syncing_quest_progress = "正在同步任务进度";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_recycling_obsession = "正在回收执念";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_game_records = "正在加载游戏记录";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_game_record_detail = "正在加载游戏记录详情";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_settling_game = "正在结算游戏";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_ranking = "正在加载排行榜";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_pvp_data = "正在加载PVP数据";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_updating_game_config = "正在更新游戏配置";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_festival = "正在加载新手活动";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_festival_draw = "正在进行新手活动抽奖";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_settling_festival_ticket = "正在结算新手活动赠票";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_claiming_festival_reward = "正在领取新手活动奖励";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_purchase_data = "正在加载商品数据";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_order_data = "正在加载订单数据";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_processing_purchase = "正在处理购买";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_card_skin_offers = "正在加载卡牌形态商店";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_refreshing_card_skin_offers = "正在刷新卡牌形态商店";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_preparing_card_skin_purchase = "正在准备卡牌形态购买";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_opening_loot = "正在开启宝箱";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_submitting_player_report = "正在提交举报";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_report_state = "正在加载举报状态";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_deleting_mail = "正在删除邮件";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_claiming_mail_attachment = "正在领取邮件附件";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_pvp_detail = "正在加载PVP详情";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_saving_character_setup = "正在保存角色配置";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_deleting_skill = "正在删除护符";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_updating_level_line = "正在更新升格之旅";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_uploading_forerunner_legacy = "正在上传先行者遗物";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_saving_name = "正在保存昵称";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_saving_forget_shrine = "正在保存忘却蜡封";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_upgrading_character = "正在升级角色";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_syncing_story_progress = "正在同步剧情进度";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_gift_code = "正在加载礼包码";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_load_game_record = "正在加载游戏记录";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_pvpModeProfileDataLoading = "正在加载PVP";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_forerunnerProfileDataLoading = "正在加载先行者数据";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_NewbieFestivalInfo = "正在加载新生游艺";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_loading_ad = "正在加载";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_ad_play_failed = "播放失败";

		[LocalizeText("通信")]
		[TextArea(1, 2)]
		public string term_order = "正在结算订单";
	}

	public enum TextTag
	{
		ReferenceItem = 8,
		ReferenceTag = 9,
		RequirementTargetCost = 11,
		NodeValue = 20,
		NodeItems = 21,
		NodeItemDefs = 22,
		NodeTag = 23,
		NodeEnchant = 24,
		times = 25,
		NodeEnchantDef = 26,
		DamgeTag = 30,
		DeltaNodeValue = 31,
		InverseDeltaNodeValue = 32,
		BoolSelect = 41,
		mCounter = 44,
		mMaxCounter = 45,
		Keyword = 46,
		mInfluenceValue = 47,
		EnchantValue = 49,
		mItemName = 50,
		AddItem = 51,
		Color = 52,
		ColorEnd = 53,
		NodeStr = 54,
		mTransformPassiveDesc = 55,
		YangPower = 58,
		YinPower = 59,
		NodeRuneDef = 60,
		NodeRune = 61,
		RuneValue = 62,
		NodeSkill = 63,
		NodeInfluence = 64,
		NodeQualityLevel = 65,
		Link = 66,
		LinkEnd = 67,
		NodePrayEffect = 68,
		Stance = 72,
		StanceDef = 73,
		ReferenceEnchant = 74
	}

	public class TextTagPreset
	{
		public TextTag type;

		public string matchRegexStr;

		public Func<string> commonTextTagGenerator;

		public Func<EffectNodeInfo, NodeDisplayInfo, string> nodeTextTagGenerator;

		public Func<TextDataGenerator, GroupCollection, string> textReplacer;
	}

	public class TextException : Exception
	{
		public enum Type
		{
			Default,
			NodeIdError,
			NoNodeTagValue
		}

		public Type type;

		public TextException(Type type, string s)
			: base(s)
		{
			this.type = type;
		}
	}

	[Serializable]
	public class Term
	{
		[SerializeField]
		public int id;

		[TextAreaProp]
		public string str
		{
			get
			{
				LocalizeString localizeString = ScriptableObjectSingleton<TextDataPreset>.instance.otherTerms.TryGetValue(id);
				if (localizeString == null)
				{
					return "----";
				}
				return localizeString.str;
			}
		}
	}

	[Serializable]
	public class ForerunnerPresetMessage
	{
		public string id;

		[LocalizeText("先行者预设留言")]
		[TextArea]
		public string text;

		public bool enabled = true;
	}

	[Serializable]
	public class GlobalItemStringConfig
	{
		[SerializeField]
		[LocalizeText("全局物品名字")]
		public string GlobalItemCellName;

		[SerializeField]
		[TextArea]
		[LocalizeText("全局物品信息框标题")]
		public string GlobalItemTitle;

		[SerializeField]
		[TextArea]
		[LocalizeText("全局物品信息框描述")]
		public string GetGlobalItemDescription;

		[SerializeField]
		[TextArea]
		[LocalizeText("全局物品信息简述")]
		public string RewardBrief;
	}

	[LocalizeText("SDK历史订单提示")]
	public string term_order_history_tips = "当遇到重置不到账情况时，请点击对应订单右侧的按钮，以校验订单并发货。每个订单每次启动只能校验一次。若校验后仍没收到购买内容，请联系客服QQ：800883683。此功能不适用于更换设备、删除游戏后导致的数据丢失情况。";

	public ForceUpdateManagerText forceUpdateManagerText = new ForceUpdateManagerText();

	public GameAsyncOperationText gameAsyncOperationText = new GameAsyncOperationText();

	[LocalizeText("零碎文本")]
	public DifficultySelectionPanel.TextConfig difficultySelectionPanelText = new DifficultySelectionPanel.TextConfig();

	[LocalizeText("幕间游艺")]
	public string term_intermission_play_dancer_task = "使用舞者通关1次";

	[LocalizeText("幕间游艺")]
	public string term_intermission_play_reclaim_success_arg0_arg1_arg2_arg3 = "回收成功：{0} ×{1}，获得{2} ×{3}";

	[LocalizeText("幕间游艺")]
	public string term_intermission_play_reclaim_reward_arg0_arg1 = "本次回收可获得：{0} ×{1}";

	[LocalizeText("幕间游艺")]
	public string term_intermission_play_reclaim_reward_arg0 = "本次回收可获得：";

	[LocalizeText("幕间游艺")]
	public string term_intermission_play_entrance_duration_arg0_days = "{0}天";

	[LocalizeText("幕间游艺")]
	public string term_intermission_play_duration_arg0_arg1_arg2_arg3 = "{0}天 {1}:{2}:{3}";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_finished = "活动已结束";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_activity_end = "活动结束";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_day_refresh = "今日刷新";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_countdown_finished_arg0 = "{0}：已结束";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_countdown_arg0_arg1 = "{0}：{1}";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_loading = "加载中";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_no_data = "暂无数据";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_drawing = "转动中";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_reward_displaying = "奖励展示中";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_draw_once = "转动一次";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_close_reward_first = "请先关闭奖励展示";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_cannot_draw = "当前不可转动";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_remaining_arg0 = "剩余 {0}";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_claim = "领取";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_go = "前往";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_special_prize = "特等奖";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_first_prize = "一等奖";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_second_prize = "二等奖";

	[LocalizeText("零碎文本")]
	public string term_newbie_festival_third_prize = "三等奖";

	[LocalizeText("零碎文本")]
	public string term_real_money_ticket = "每充值{0}60或等值礼包获得1张{1}";

	[LocalizeText("零碎文本")]
	public string _term_AddSlotRange = "当前孔位数可提升至<b><color=#FFEB92>{0}</color>~<color=#FFEB92>{1}</color></b>孔";

	[LocalizeText("零碎文本")]
	public string _term_AddSlotTo = "当前孔位数可提升至<b><color=#FFEB92>{0}</color></b>孔";

	[LocalizeText("零碎文本")]
	public string _term_MaxSlot = "当前孔位数已提升至<b><color=#A0F4FF>最大</color></b>";

	[LocalizeText("零碎文本")]
	public string _term_AddSlot = "打孔强化";

	[LocalizeText("零碎文本")]
	public string _term_ChangeSlot = "重新打孔";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_title = "每日挑战";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_back = "返回星图";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_activity_help = "查看活动玩法说明";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_constellation_placeholder = "每日挑战星座";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_completion_reward = "挑战完成奖励";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_reward_name = "热诚微光";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_reward_status_arg0_arg1 = "本周剩余奖励次数：{1}/{0}(每日限1次)";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_weekly_countdown = "本周奖励倒计时：";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_weekly_countdown_arg0 = "本周倒计时：{0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_today_node = "今日节点";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_rotation_countdown_arg0 = "玩法轮换倒计时  {0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_daily_countdown = "每日刷新倒计时：";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_remaining_arg0 = "今日挑战剩余：{0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_gameplay_contract = "玩法规则契约";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_score_contract = "得分规则契约";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_score_competition_title = "得分竞赛契约";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_score_breakdown_title = "每日挑战得分";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_contract_missing = "当前挑战未找到可用契约";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_start = "开始";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_ranking = "排行榜";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_settlement_countdown_arg0 = "结算倒计时  {0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_ranking_help = "查看排行榜奖励说明";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_ranking_header = "排名    头像    名称 / 使用角色          难度        得分";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_ranking_rank = "排名";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_ranking_player = "玩家信息";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_ranking_difficulty = "难度";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_ranking_score = "得分";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_self_ranking = "我的排行";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_no_record = "尚未进行今日挑战，暂无排行记录";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_unchallenged = "未挑战";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_activity_help_title = "活动玩法说明";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_ranking_help_title = "排行榜奖励说明";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_challenge_reward_help_title = "挑战奖励";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_gameplay_help_title = "挑战规则";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_ranking_reward_help_title = "排行奖励";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_reward_rules_help_title = "排行说明";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_help_missing = "帮助文本资源未加载";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_loading = "加载中";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_load_failed = "每日挑战数据加载失败";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_record_load_failed = "挑战记录加载失败，请稍后重试";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_retry = "重试";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_close = "关闭";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_first_reward_arg0_arg1 = "第1名 {0} x {1}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_top_reward_arg0_arg1_arg2 = "前{0}名 {1} x {2}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_first_place = "第 1 名";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_top_rank_arg0 = "前 {0} 名";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_duration_days_arg0_arg1 = "{0}天 {1}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_score_method_arg0_arg1 = "{0}\n计分方式：{1}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_score_breakdown_arg0_arg1_arg2_arg3_arg4_arg5 = "通关基础经验      {0}\n竞赛契约得分      {1}\n倍率前得分        {2}\nStage 7 加成      x {3}\n难度系数          x {4}\n最终得分          {5}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_settlement_title_arg0 = "{0}（每日挑战）";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_score_contract_fallback = "竞赛契约";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_target_arg0 = "目标：{0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_rule_arg0 = "规则：{0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_calculation_arg0 = "本局代入：{0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_bonus_arg0 = "加成：{0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_final_score_arg0 = "最终得分：{0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_boss_bonus_arg0 = "Stage 7 Boss x{0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_difficulty_bonus_arg0 = "难度 x{0}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_completed = "每日挑战已完成";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_not_completed = "每日挑战未完成";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_contract_expired = "活动契约已过期，本局不计入排行";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_settlement = "每日挑战结算";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_failed_score = "挑战失败 = 0";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_failed_final_score = "挑战失败，最终得分为 0";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_final_formula_arg0_arg1_arg2_arg3 = "({0} + {1}) x {2} x {3}";

	[LocalizeText("零碎文本")]
	public string term_daily_challenge_default_player_name = "我的名字";

	[LocalizeText("帮助正文")]
	[TextArea(4, 12)]
	public string term_daily_challenge_activity_help_page_1 = "挑战时间\n\n每日挑战于 GMT+8 04:00 刷新，并生成新的挑战契约。\n刷新后，前一日排行榜将立即封榜。超过结算时间完成的挑战，不会计入排行榜，但符合条件时仍可获得基础通关奖励。";

	[LocalizeText("帮助正文")]
	[TextArea(4, 12)]
	public string term_daily_challenge_activity_help_page_2 = "挑战契约\n\n每日挑战由以下两类契约随机组合而成：\n\n玩法规则契约：改变本次挑战的基础玩法规则。\n得分竞赛契约：决定本次挑战的计分方式。\n\n玩家需要在指定玩法规则下完成挑战，并根据当日得分竞赛契约争取更高分数。";

	[LocalizeText("帮助正文")]
	[TextArea(4, 12)]
	public string term_daily_challenge_activity_help_page_3 = "基础挑战奖励\n\n每周最多可获得 3 次每日挑战基础奖励。\n玩家在仍有奖励次数时，首次完成某日挑战可获得：\n\n热诚微光 x 600\n\n同一日可以重复挑战，但基础奖励只能获得一次。\n每周已领取 3 次基础奖励后，继续完成挑战仍可参与排行榜，但不再获得基础奖励。";

	[LocalizeText("帮助正文")]
	[TextArea(4, 12)]
	public string term_daily_challenge_ranking_help_page_1 = "排行榜计分\n\n挑战失败时，本次得分为 0 分。\n挑战成功时，最终得分按照以下公式计算：\n\n最终得分 =（通关基础经验 + 竞赛契约得分）x 额外加成 x 难度系数\n\n击败 Stage 7 最终首领「月之茧·坎迪斯」时，本次挑战的额外加成为 1.5 倍。";

	[LocalizeText("帮助正文")]
	[TextArea(4, 12)]
	public string term_daily_challenge_ranking_help_page_2 = "排名规则\n\n排行榜仅记录玩家在当前每日挑战中的最高有效得分。\n玩家可以重复挑战。若新成绩高于当前最佳成绩，排行榜将更新为新的成绩；较低成绩不会覆盖已有最佳成绩。";

	[LocalizeText("帮助正文")]
	[TextArea(4, 12)]
	public string term_daily_challenge_ranking_help_page_3 = "排行榜奖励\n\n每日排行榜结算后，达到指定排名的玩家可获得排行榜奖励。\n\n前 10 名：热诚微光 x 900\n前 100 名：热诚微光 x 600\n前 1000 名：热诚微光 x 300\n\n排行榜奖励不受基础奖励次数限制，并将在结算后通过邮件发放。\n\n排行榜奖励的名次范围可能根据当日参与人数调整，请以当日活动页面显示为准。";

	public const string referenceItemTagRegex = "(?<!.)rit=(?<id>\\w+)(?!\\w)";

	public const string referenceEnchantTagRegex = "(?<!.)rec=(?<id>\\w+)(?!\\w)";

	public const string referenceTagTagRegex = "(?<!.)rtg=(?<id>\\w+)(?!\\w)";

	public const string requirementTargetCostRegex = "(?<!.)ReqTC(?!\\w)";

	public const string nodeValueRegex = "(?<!.)val=(?<id>\\d+)(?!\\w)";

	public const string yangPowerRegex = "(?<!.)yangPower(?!\\w)";

	public const string yinPowerRegex = "(?<!.)yinPower(?!\\w)";

	public const string nodeItemsRegex = "(?<!.)it=(?<id>\\d+)(?!\\w)";

	public const string nodeItemDefsRegex = "(?<!.)itd=(?<id>\\d+)(?!\\w)";

	public const string nodeTagRegex = "(?<!.)tg=(?<id>\\d+)(?!\\w)";

	public const string nodeEnchantsRegex = "(?<!.)ec=(?<id>\\d+)(?!\\w)";

	public const string nodeEnchantDefsRegex = "(?<!.)ecd=(?<id>\\d+)(?!\\w)";

	public const string nodeRuneRegex = "(?<!.)rune=(?<id>\\d+)(?!\\w)";

	public const string nodeRuneDefRegex = "(?<!.)runeDef=(?<id>\\d+)(?!\\w)";

	public const string nodeStrRegex = "(?<!.)str=(?<id>\\d+)(?!\\w)";

	public const string damageRegex = "(?<!.)da=(?<id>\\d+)(?!\\w)";

	public const string deltaNodeValueRegex = "(?<!.)dval=(?<id>\\d+)(?!\\w)";

	public const string nodeSkillRegex = "(?<!.)skill=(?<id>\\d+)(?!\\w)";

	public const string nodeInfluenceRegex = "(?<!.)influence=(?<id>\\d+)(?!\\w)";

	public const string nodeQualityLevelRegex = "(?<!.)quality=(?<id>\\d+)(?!\\w)";

	public const string inverseDeltaNodeValueRegex = "(?<!.)idval=(?<id>\\d+)(?!\\w)";

	public const string nodePrayEffectRegex = "(?<!.)prayEffect=(?<id>\\d+)(?!\\w)";

	public const string nodeStanceRegex = "(?<!.)stance=(?<id>\\d+)(?!\\w)";

	public const string nodeStanceDefRegex = "(?<!.)stanceDef=(?<id>\\d+)(?!\\w)";

	public const string boolSelectRegex = "(?<!.)bool=(?<id>\\d+)\\?(?<Tstr>[\\s\\S]*?):(?<Fstr>[\\s\\S]*)(?!\\w)";

	public const string mCounterRegex = "(?<!.)mCounter(?!\\w)";

	public const string mInfluenceValueRegex = "(?<!.)mInfluenceValue(?!\\w)";

	public const string mMaxCounterRegex = "(?<!.)mMaxCounter(?!\\w)";

	public const string mItemNameRegex = "(?<!.)mItemName(?!\\w)";

	public const string mTransformPassiveDescRegex = "(?<!.)mTransformPassiveDesc(?!\\w)";

	public const string timesRegex = "(?<!.)times=(?<id>\\d+)(?!\\w)";

	public const string enchantValueRegex = "(?<!.)enchantValue(?!\\w)";

	public const string runeValueRegex = "(?<!.)runeValue(?!\\w)";

	public const string addItemsRegex = "(?<!.)ait=(?<id>\\d+)(?!\\w)";

	public const string colorRegex = "(?<!.)color=(?<str>.*)";

	public const string colorEndRegex = "(?<!.)/color(?!\\w)";

	public const string linkRegex = "(?<!.)link=(?<str>.*)";

	public const string linkEndRegex = "(?<!.)/link(?!\\w)";

	public const string UnDefinedValue = "？";

	public const string TextFormatErrorMark = "!!!节点文本错误!!!";

	public static readonly Regex groupRegex = new Regex("<(<(?<n>)|>(?<-n>)|(?!<|>)[\\s\\S])*(?(n)(?!))>");

	public static TextTagPreset keywordTagPreset = new TextTagPreset
	{
		type = TextTag.Keyword,
		matchRegexStr = null,
		textReplacer = delegate(TextDataGenerator tdg, GroupCollection gc)
		{
			string id = gc["id"].Value;
			KeywordDef keywordDef = tdg.gameState.applicationState.assetCatalog.keywordCatalog.assets.Find((KeywordDef k) => k.textId == id);
			tdg.refLinkObjects.Add(keywordDef.displayNode.GenerateKeywordInfo(keywordDef, gc, tdg.displayGameEvent));
			return keywordDef.GetKeywordStr(gc, tdg.displayGameEvent, tdg.refLinkObjects.Count - 1);
		}
	};

	public static TextTagPreset[] textTagPresets = new TextTagPreset[38]
	{
		new TextTagPreset
		{
			type = TextTag.RequirementTargetCost,
			matchRegexStr = "(?<!.)ReqTC(?!\\w)",
			commonTextTagGenerator = () => "<ReqTC>",
			textReplacer = (TextDataGenerator tdg, GroupCollection gc) => ""
		},
		new TextTagPreset
		{
			type = TextTag.ReferenceItem,
			matchRegexStr = "(?<!.)rit=(?<id>\\w+)(?!\\w)",
			commonTextTagGenerator = () => "<rit=#id#>",
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				ItemDefinition asset = td.gameState.applicationState.assetCatalog.itemDefCatalog.GetAsset(gc["id"].Value);
				if (asset == null)
				{
					throw new TextException(TextException.Type.NoNodeTagValue, $"{TextTag.ReferenceItem}");
				}
				return td.GetItemDefinitionText(asset);
			}
		},
		new TextTagPreset
		{
			type = TextTag.ReferenceEnchant,
			matchRegexStr = "(?<!.)rec=(?<id>\\w+)(?!\\w)",
			commonTextTagGenerator = () => "<rec=#id#>",
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				EnchantDef asset = td.gameState.applicationState.assetCatalog.enchantDefCatalog.GetAsset(gc["id"].Value);
				if (asset == null)
				{
					throw new TextException(TextException.Type.NoNodeTagValue, $"{TextTag.ReferenceEnchant}");
				}
				return td.GetEnchantDefText(new EnchantDef[1] { asset });
			}
		},
		new TextTagPreset
		{
			type = TextTag.ReferenceTag,
			matchRegexStr = "(?<!.)rtg=(?<id>\\w+)(?!\\w)",
			commonTextTagGenerator = () => "<rtg=#id#>",
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				ItemTag asset = td.gameState.applicationState.assetCatalog.itemTagDefCatalog.GetAsset(gc["id"].Value);
				return td.GetTagsText(new ItemTag[1] { asset });
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeValue,
			matchRegexStr = "(?<!.)val=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.value != null) ? $"<val={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo nodeDisplayInfo = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent);
				NodeDisplayInfo.Element<int?> value = nodeDisplayInfo.value;
				if (!value.valid)
				{
					return string.Format(ValueFormat, "？");
				}
				string text = string.Format(ValueFormat, Mathf.Abs(value.value.Value));
				NodeDisplayInfo.Element<int?> deltaValue = nodeDisplayInfo.deltaValue;
				if (deltaValue != null && deltaValue.value > 0)
				{
					text += ScriptableObjectSingleton<GameDataManager>.instance.increaseTextIcon.ToTMPSpriteText();
				}
				return text;
			}
		},
		new TextTagPreset
		{
			type = TextTag.YinPower,
			matchRegexStr = "(?<!.)yinPower(?!\\w)",
			commonTextTagGenerator = () => "<yinPower>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => ScriptableObjectSingleton<GameDataManager>.instance.yinTextIcon.ToTMPSpriteText()
		},
		new TextTagPreset
		{
			type = TextTag.YangPower,
			matchRegexStr = "(?<!.)yangPower(?!\\w)",
			commonTextTagGenerator = () => "<yangPower>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => ScriptableObjectSingleton<GameDataManager>.instance.yangTextIcon.ToTMPSpriteText()
		},
		new TextTagPreset
		{
			type = TextTag.NodeItems,
			matchRegexStr = "(?<!.)it=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refItems != null) ? $"<it={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<List<Item>> refItems = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).refItems;
				return td.GetItemsText(refItems.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeItemDefs,
			matchRegexStr = "(?<!.)itd=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refItemDefs != null) ? $"<itd={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<List<ItemDefinition>> refItemDefs = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).refItemDefs;
				return td.GetItemDefsText(refItemDefs.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeEnchant,
			matchRegexStr = "(?<!.)ec=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refEnchants != null) ? $"<ec={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo nodeDisplayInfo = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent);
				return td.GetEnchantsText(nodeDisplayInfo.refEnchants.value, nodeDisplayInfo.deltaValue?.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeEnchantDef,
			matchRegexStr = "(?<!.)ecd=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refEnchantDefs != null) ? $"<ecd={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<List<EnchantDef>> refEnchantDefs = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).refEnchantDefs;
				return td.GetEnchantDefText(refEnchantDefs.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeRune,
			matchRegexStr = "(?<!.)rune=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refRunes != null) ? $"<rune={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<List<Rune>> refRunes = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).refRunes;
				return td.GetRunesText(refRunes.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeRuneDef,
			matchRegexStr = "(?<!.)runeDef=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refRuneDefs != null) ? $"<runeDef={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				List<Rune> runes = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).refRuneDefs.value.ConvertAll((RuneDef rd) => new Rune(rd));
				return td.GetRunesText(runes);
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeTag,
			matchRegexStr = "(?<!.)tg=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refTags != null) ? $"<tg={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<List<ItemTag>> refTags = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).refTags;
				return td.GetTagsText(refTags.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.DamgeTag,
			matchRegexStr = "(?<!.)da=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.damage != null) ? $"<da={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo nodeDisplayInfo = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode?.GetNodeDisplayInfo(td.displayGameEvent);
				return td.GetDamageText(nodeDisplayInfo.baseValue.value, nodeDisplayInfo.deltaValue.value, nodeDisplayInfo.times.value, nodeDisplayInfo.deltaTimes.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.DeltaNodeValue,
			matchRegexStr = "(?<!.)dval=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.value != null) ? $"<dval={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<int?> element = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode?.GetNodeDisplayInfo(td.displayGameEvent).value;
				return (!element.valid) ? "？" : string.Format(ValueFormat, TextDisplayHelper.NumToStringWithSign(element.value.Value));
			}
		},
		new TextTagPreset
		{
			type = TextTag.InverseDeltaNodeValue,
			matchRegexStr = "(?<!.)idval=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.value != null) ? $"<idval={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<int?> element = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode?.GetNodeDisplayInfo(td.displayGameEvent).value;
				return (!element.valid) ? "？" : string.Format(ValueFormat, TextDisplayHelper.NumToStringWithSign(-element.value.Value));
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeSkill,
			matchRegexStr = "(?<!.)skill=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refSkills != null) ? $"<skill={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<List<GameSkill>> refSkills = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).refSkills;
				return td.GetSkillsText(refSkills.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeInfluence,
			matchRegexStr = "(?<!.)influence=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refInfluences != null) ? $"<influence={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<List<Influence>> refInfluences = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).refInfluences;
				return td.GetInfluencesText(refInfluences.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeQualityLevel,
			matchRegexStr = "(?<!.)quality=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.qualityLevel != null) ? $"<quality={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<QualityLevel> qualityLevel = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).qualityLevel;
				return ScriptableObjectSingleton<TextDataPreset>.instance.GetQualityLevelText(qualityLevel.value).ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(qualityLevel.value));
			}
		},
		new TextTagPreset
		{
			type = TextTag.BoolSelect,
			matchRegexStr = "(?<!.)bool=(?<id>\\d+)\\?(?<Tstr>[\\s\\S]*?):(?<Fstr>[\\s\\S]*)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.outputBool != null) ? $"<bool={nd.index}?T:F>" : null,
			textReplacer = (TextDataGenerator td, GroupCollection gc) => (td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).outputBool.value != true) ? gc["Fstr"].Value.NoBreak() : gc["Tstr"].Value.NoBreak()
		},
		new TextTagPreset
		{
			type = TextTag.mCounter,
			matchRegexStr = "(?<!.)mCounter(?!\\w)",
			commonTextTagGenerator = () => "<mCounter>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => string.Format(ValueFormat, td.displayGameEvent.CasterItem.counter.ToString())
		},
		new TextTagPreset
		{
			type = TextTag.mInfluenceValue,
			matchRegexStr = "(?<!.)mInfluenceValue(?!\\w)",
			commonTextTagGenerator = () => "<mInfluenceValue>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => string.Format(ValueFormat, td.displayGameEvent.CasterInfluence.influenceValue.ToString())
		},
		new TextTagPreset
		{
			type = TextTag.mMaxCounter,
			matchRegexStr = "(?<!.)mMaxCounter(?!\\w)",
			commonTextTagGenerator = () => "<mMaxCounter>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => string.Format(ValueFormat, td.displayGameEvent.CasterItem.maxCounter.ToString())
		},
		new TextTagPreset
		{
			type = TextTag.mItemName,
			matchRegexStr = "(?<!.)mItemName(?!\\w)",
			commonTextTagGenerator = () => "<mItemName>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => td.GetDisplayItemName(td.displayGameEvent.CasterItem).ColorText(ScriptableObjectSingleton<TextDataPreset>.instance.linkItemColor)
		},
		new TextTagPreset
		{
			type = TextTag.times,
			matchRegexStr = "(?<!.)times=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.times != null) ? $"<times={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<int?> times = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).times;
				return string.Format(ValueFormat, times.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.mTransformPassiveDesc,
			matchRegexStr = "(?<!.)mTransformPassiveDesc(?!\\w)",
			commonTextTagGenerator = () => "<mTransformPassiveDesc>",
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				if (!(td.textSource is Item { HasTransformTemplate: not false } item))
				{
					return "";
				}
				if (item.transformActive)
				{
					string text = td.Parse(GetItemOriginalSpellDescriptionRaw(item) ?? "");
					return td.Parse(ScriptableObjectSingleton<TextDataPreset>.instance.term_transform_active_original_arg0.FormatWith(text));
				}
				if (item.runtimeTransformSpellTemplate != null && item.transformSpellSourceItem != null)
				{
					string itemDisplayReferenceText = td.GetItemDisplayReferenceText(item.transformSpellSourceItem);
					return td.Parse(ScriptableObjectSingleton<TextDataPreset>.instance.term_transform_source_arg0.FormatWith(itemDisplayReferenceText));
				}
				string text2 = td.Parse(GetItemTransformSpellDescriptionRaw(item) ?? "");
				return (!string.IsNullOrEmpty(text2)) ? td.Parse(ScriptableObjectSingleton<TextDataPreset>.instance.term_transform_source_arg0.FormatWith(text2)) : "";
			}
		},
		new TextTagPreset
		{
			type = TextTag.EnchantValue,
			matchRegexStr = "(?<!.)enchantValue(?!\\w)",
			commonTextTagGenerator = () => "<enchantValue>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => ValueFormat.FormatWith(td.displayGameEvent.CasterEnchant.value.ToString() ?? "")
		},
		new TextTagPreset
		{
			type = TextTag.RuneValue,
			matchRegexStr = "(?<!.)runeValue(?!\\w)",
			commonTextTagGenerator = () => "<runeValue>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => ValueFormat.FormatWith(td.displayGameEvent.CasterRune.value.ToString() ?? "")
		},
		new TextTagPreset
		{
			type = TextTag.AddItem,
			matchRegexStr = "(?<!.)ait=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.addItemInfo != null) ? $"<ait={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo nodeDisplayInfo = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent);
				return td.GetAddItemText(nodeDisplayInfo.addItemInfo.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.NodeStr,
			matchRegexStr = "(?<!.)str=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.str != null) ? $"<str={nd.index}>" : null,
			textReplacer = (TextDataGenerator td, GroupCollection gc) => td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).str.value
		},
		new TextTagPreset
		{
			type = TextTag.Color,
			matchRegexStr = "(?<!.)color=(?<str>.*)",
			commonTextTagGenerator = () => "<color=#str#>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => string.Format("<color={0}>", gc["str"])
		},
		new TextTagPreset
		{
			type = TextTag.ColorEnd,
			matchRegexStr = "(?<!.)/color(?!\\w)",
			commonTextTagGenerator = () => "</color>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => "</color>"
		},
		new TextTagPreset
		{
			type = TextTag.Link,
			matchRegexStr = "(?<!.)link=(?<str>.*)",
			commonTextTagGenerator = () => "<link=#str#>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => string.Format("<link={0}>", gc["str"])
		},
		new TextTagPreset
		{
			type = TextTag.LinkEnd,
			matchRegexStr = "(?<!.)/link(?!\\w)",
			commonTextTagGenerator = () => "</link>",
			textReplacer = (TextDataGenerator td, GroupCollection gc) => "</link>"
		},
		new TextTagPreset
		{
			type = TextTag.NodePrayEffect,
			matchRegexStr = "(?<!.)prayEffect=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.pray != null) ? $"<prayEffect={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo.Element<Pray> pray = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent).pray;
				TextDataGenerator generator;
				return (!pray.valid) ? "" : pray.value.GetDescription(td.gameState, out generator);
			}
		},
		new TextTagPreset
		{
			type = TextTag.Stance,
			matchRegexStr = "(?<!.)stance=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refStance != null) ? $"<stance={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo nodeDisplayInfo = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent);
				return td.GetStanceText(nodeDisplayInfo.refStance.value, nodeDisplayInfo.deltaValue.value);
			}
		},
		new TextTagPreset
		{
			type = TextTag.StanceDef,
			matchRegexStr = "(?<!.)stanceDef=(?<id>\\d+)(?!\\w)",
			nodeTextTagGenerator = (EffectNodeInfo nd, NodeDisplayInfo ind) => (ind?.refStanceDef != null) ? $"<stanceDef={nd.index}>" : null,
			textReplacer = delegate(TextDataGenerator td, GroupCollection gc)
			{
				NodeDisplayInfo nodeDisplayInfo = td.textSource.TryGetNodeDisplayInfoOfId(gc["id"].Value).referenceNode.GetNodeDisplayInfo(td.displayGameEvent);
				return td.GetStanceDefText(nodeDisplayInfo.refStanceDef.value);
			}
		}
	};

	public static string ValueFormat = "<nobr><color=#4ca5ed>{0}</color></nobr>";

	[LocalizeText("零碎文本")]
	public string term_difficulty_arg0 = "难度：{0}";

	[LocalizeText("零碎文本")]
	public string term_difficulty_bonus_multiplier_arg0 = "奖励倍数：{0:0.#}";

	[LocalizeText("零碎文本")]
	public string term_unlock_difficulty_with_arg0 = "完成{0}难度解锁";

	[LocalizeText("零碎文本")]
	public string term_difficulty_finish_reward = "完成奖励";

	[LocalizeText("零碎文本")]
	public string term_difficulty_account_finish_reward = "首次通关可得";

	[LocalizeText("零碎文本")]
	public string term_difficulty_class_arg0_finish_reward = "{0}首通可得";

	[LocalizeText("零碎文本")]
	public string term_encounter_arg0 = "<color=#eb3c65>遭遇</color><color=#eeeeee>{0}</color>";

	[LocalizeText("零碎文本")]
	public string term_your_turn = "<color=#f1da8c>你的回合</color>";

	[LocalizeText("零碎文本")]
	public string term_turn_count_arg0_colored = "<color=#b4b1a4>第{0}回合</color>";

	[LocalizeText("零碎文本")]
	public string term_turn_count_arg0 = "第{0}回合";

	[LocalizeText("零碎文本")]
	public string term_opponent_turn = "<color=#ee9cef>敌人回合</color>";

	[LocalizeText("零碎文本")]
	public string term_enemy_ability_acitve_in_phase_2 = "【二阶段生效】";

	[LocalizeText("零碎文本")]
	public string term_stance_shift = "切架！";

	[LocalizeText("零碎文本")]
	public string term_stance_full = "满架！";

	[LocalizeText("零碎文本")]
	public string term_stance_stack = "叠架！";

	[LocalizeText("零碎文本")]
	public string term_stance_break = "破架！";

	[LocalizeText("零碎文本")]
	public string term_stance_shift_and_full = "切架/满架！";

	[LocalizeText("零碎文本")]
	public string term_stance_stack_and_full = "叠架/满架！";

	[LocalizeText("零碎文本")]
	public string term_item_table_hard_limit_reached = "卡牌数量已达上限";

	[LocalizeText("零碎文本")]
	public string term_trigrams_yin = "阴";

	[LocalizeText("零碎文本")]
	public string term_trigrams_yang = "阳";

	[LocalizeText("零碎文本")]
	public string term_trigrams_extreme_yin = "极阴";

	[LocalizeText("零碎文本")]
	public string term_trigrams_extreme_yang = "极阳";

	[LocalizeText("零碎文本")]
	public string term_trigrams_extreme_yin_description = "极阴：受到伤害降低50%。";

	[LocalizeText("零碎文本")]
	public string term_trigrams_extreme_yang_description = "极阳：伤害增加50%。";

	[LocalizeText("零碎文本")]
	public string term_select_one_exiled_card_to_draw_pile_top = "选择1张已消除的卡牌放回队首";

	[LocalizeText("零碎文本")]
	public string term_transform_active_original_arg0 = "<Transform>中，原型：{0}";

	[LocalizeText("零碎文本")]
	public string term_transform_source_arg0 = "<Transform>：{0}";

	[LocalizeText("零碎文本")]
	public string term_map_state_explored = "已探索";

	[LocalizeText("零碎文本")]
	public string term_map_state_current = "当前";

	[LocalizeText("零碎文本")]
	public string term_map_state_unexplored = "未探索";

	[LocalizeText("零碎文本")]
	public string term_map_unknown_area = "未知区域";

	[LocalizeText("零碎文本")]
	public string term_select_bonus = "选择奖励";

	[LocalizeText("零碎文本")]
	public string term_switch_another = "换一个";

	[LocalizeText("零碎文本")]
	public string term_remain_chance_count_arg0 = "剩余 {0} 次机会";

	[LocalizeText("零碎文本")]
	public string term_stop = "停止";

	[LocalizeText("零碎文本")]
	public string term_shellGame_choose_bonus_arg0_per_arg1 = "选择你的奖励 {0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_start_exciting = "开始！";

	[LocalizeText("零碎文本")]
	public string term_bonus = "奖励";

	[LocalizeText("零碎文本")]
	public string term_select_item = "选择卡牌";

	[LocalizeText("零碎文本")]
	public string term_select_item_as_cost = "选择卡牌作为代价";

	[LocalizeText("零碎文本")]
	public string term_get_item = "卡牌";

	[LocalizeText("零碎文本")]
	public string term_loot_item = "战利品";

	[LocalizeText("零碎文本")]
	public string term_get_rare_item = "稀有卡牌";

	[LocalizeText("零碎文本")]
	public string term_get_coin = "金币";

	[LocalizeText("零碎文本")]
	public string term_get_args_coin = "获得{0}金币";

	[LocalizeText("零碎文本")]
	public string term_get_crystal = "魂晶";

	[LocalizeText("零碎文本")]
	public string term_get_args_crystal = "获得{0}魂晶";

	[LocalizeText("零碎文本")]
	public string term_replenish_usage_count_arg0 = "使用次数+{0}";

	[LocalizeText("零碎文本")]
	public string term_gem = "宝石";

	[LocalizeText("零碎文本")]
	public string term_gem_combine = "宝石";

	[LocalizeText("零碎文本")]
	public string term_mercy_get_crystal = "垂怜";

	[LocalizeText("零碎文本")]
	public string term_mercy_get_crystal_arg0_now = "立即获得 {0} 魂晶";

	[LocalizeText("零碎文本")]
	public string term_heal_all_health = "恢复所有生命";

	[LocalizeText("零碎文本")]
	public string term_heal_health = "恢复生命";

	[LocalizeText("零碎文本")]
	public string term_add_max_health = "增加最大生命";

	[LocalizeText("零碎文本")]
	public string term_heal_health_arg0 = "恢复{0}生命";

	[LocalizeText("零碎文本")]
	public string term_influence_bonus = "祝福奖励";

	[LocalizeText("零碎文本")]
	public string term_boss_influence_bonus = "强大祝福奖励";

	[LocalizeText("零碎文本")]
	public string term_add_itemSlot_bonus = "手牌上限+1";

	[LocalizeText("零碎文本")]
	public string term_draw_item_count_bonus = "抽牌数+1";

	[LocalizeText("零碎文本")]
	public string term_upgrade_one_item = "升级1张卡牌";

	[LocalizeText("零碎文本")]
	public string term_remove_one_item = "升级1张卡牌";

	[LocalizeText("零碎文本")]
	public string term_attach_arg0_rune = "刻写-{0}";

	[LocalizeText("零碎文本")]
	public string term_contract_got_finish_bonus_count_arg0_per_all_bonus_count_arg1 = "已获得的难度奖励：{0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_battle_win = "战斗胜利";

	[LocalizeText("零碎文本")]
	public string term_battle_lose = "战斗失败";

	[LocalizeText("零碎文本")]
	public string term_battle_escape = "逃离战斗";

	[LocalizeText("零碎文本")]
	public string term_you_are_dead = "你死了";

	[LocalizeText("零碎文本")]
	public string term_gameOver_win = "仪式完成";

	[LocalizeText("零碎文本")]
	public string term_gameOver_lose = "仪式失败";

	[LocalizeText("零碎文本")]
	public string term_gameOver_win_fake_ending = "仪式终结？";

	[LocalizeText("零碎文本")]
	public string term_death_pending_give_up = "放弃";

	[LocalizeText("零碎文本")]
	public string term_achievement_title = "成就";

	[LocalizeText("零碎文本")]
	public string term_achievement_lock = "未达成";

	[LocalizeText("零碎文本")]
	public string term_achievement_achieve_arg0 = "达成 {0}";

	[LocalizeText("零碎文本")]
	public string term_achievement_get_bonus = "领取";

	[LocalizeText("零碎文本")]
	public string term_achievement_bonus_got = "已领取";

	[LocalizeText("零碎文本")]
	public string term_achievement_finished_date_arg0 = "完成日期：{0}";

	[LocalizeText("零碎文本")]
	public string term_achievement_achieve_count_arg0 = "达成次数：{0}";

	[LocalizeText("零碎文本")]
	public string term_quest_get_bonus = "领取";

	[LocalizeText("零碎文本")]
	public string term_quest_jump_to_Panel = "前往";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_mission_yuanqi_01_title = "元气降临";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_mission_yuanqi_01_description = "兑换特定礼品码";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_mission_yuanqi_02_title = "初次探索";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_mission_yuanqi_02_description = "使用小骑士游玩一局并在结算页分享";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_mission_yuanqi_03_title = "天赋觉醒";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_mission_yuanqi_03_description = "活动期内学习小骑士的 10 个天赋";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_mission_yuanqi_04_title = "突破深境";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_mission_yuanqi_04_description = "使用小骑士在噩梦及以上难度击败月之茧-坎迪丝";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_yuanqi_entrance_open_after_arg0_days = "{0}天后开放";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_yuanqi_entrance_remain_arg0_days = "剩余{0}天";

	[LocalizeText("元气骑士联动活动")]
	public string term_activity_yuanqi_not_open_yet_remain_arg0_days = "活动尚未开启，剩余开启时间 {0} 天";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_refresh_in_arg0_time_span = "{0}后刷新任务";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_win_game_with_character_arg0_times_arg1 = "使用角色 {0} 赢得 {1} 局游戏";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_win_game_with_skill_arg0_times_arg1 = "使用护符 {0} 赢得 {1} 局游戏";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_finish_game_arg0_times = "赢得 {0} 局游戏";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_activate_skill_arg0_times_arg1 = "激活护符 {0} 次";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_kill_arg0_count_enemy = "击败 {0} 个敌人";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_with_activating_skill_arg0_kill_arg1_count_enemy = "在激活 {0} 护符时击败 {1} 个敌人";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_use_character_arg0_kill_arg1_count_enemy = "使用角色 {0} 击败 {1} 个敌人";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_kill_arg0_count_boss = "击败 {0} 个首领敌人";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_deal_arg0_damage_sum = "累计造成 {0} 点伤害";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_use_character_arg0_deal_arg1_damage = "使用角色 {0} 造成 {1} 点伤害";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_get_arg0_coin_sum = "累计获得 {0} 金币";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_get_arg0_crystal_sum = "累计获得 {0} 魂晶";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_get_arg0_influence_sum = "累计获得 {0} 个祝福";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_mintage_arg0_rune_sum = "累计附加 {0} 个印记";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_cast_count_arg0_of_card_arg1 = "累计使用 {0} 张 {1}";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_enter_location_arg0_count_sum = "累计进入 {0} 个地点";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_enter_location_arg0_type_arg1_count_sum = "累计进入 {0} 地点 {1} 次";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_defend_damage_arg0_sum = "护甲累计抵挡 {0} 点伤害";

	[LocalizeText("零碎文本")]
	public string term_routineQuest_upgrade_item_count_arg0_sum = "累计升级 {0} 张卡牌";

	[LocalizeText("零碎文本")]
	public string term_daily_quest_replace_count_remain_arg0_per_max_arg1 = "剩余每日刷新次数： <size=130%><color=#FFFEDD>{0}<size=80%><color=#D8D7C4>/{1}</color> ";

	[LocalizeText("零碎文本")]
	public string term_this_week_season_exp_limit_arg0_per_arg1 = "本周经验上限：{0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_statistics_game_count = "游戏次数";

	[LocalizeText("零碎文本")]
	public string term_need_clavis_to_unlock = "（钥匙解锁）";

	[LocalizeText("零碎文本")]
	public string term_need_repaired_final_ending_key_to_unlock = "（需要修复的时之钥）";

	[LocalizeText("零碎文本")]
	public string term_statistics_win_count = "完成游戏次数";

	[LocalizeText("零碎文本")]
	public string term_statistics_enemy_defeat_count = "击败敌人数";

	[LocalizeText("零碎文本")]
	public string term_statistics_event_encounter_count = "遭遇事件数";

	[LocalizeText("零碎文本")]
	public string term_statistics_cast_item_count = "施放卡牌总数";

	[LocalizeText("零碎文本")]
	public string term_statistics_damage_count = "造成伤害总数";

	[LocalizeText("零碎文本")]
	public string term_statistics_gain_coin_count = "获得金币总数";

	[LocalizeText("零碎文本")]
	public string term_statistics_gain_crystal_count = "获得魂晶总数";

	[LocalizeText("零碎文本")]
	public string term_statistics_global = "全局";

	[LocalizeText("零碎文本")]
	public string term_statistics_highest_finished_difficulty = "最高通关难度";

	[LocalizeText("零碎文本")]
	public string term_statistics_current_win_streak = "当前连胜";

	[LocalizeText("零碎文本")]
	public string term_statistics_max_win_streak = "最高连胜";

	[LocalizeText("零碎文本")]
	public string term_statistics_difficulty_record_arg0_arg1_arg2 = "{0}：游戏 {1} / 胜利 {2}";

	[LocalizeText("零碎文本")]
	public string term_append_text_use_clavis_need_arg0 = "（需要 {0}）";

	[LocalizeText("零碎文本")]
	public string term_append_text_use_clavis_with_arg0 = "（使用 {0}）";

	[LocalizeText("零碎文本")]
	public string term_unlock_clavis_for_activate = "获得关键道具";

	[LocalizeText("零碎文本")]
	public string term_activate_clavis_effect = "关键道具已激活";

	[LocalizeText("零碎文本")]
	public string term_continue_game_find_clavis_to_unlock = "继续探索，寻找「{0}」解锁";

	[LocalizeText("零碎文本")]
	public string term_forerunner_leave_legacy_hint_1 = "你要，留下，些什么";

	[LocalizeText("零碎文本")]
	public string term_forerunner_leave_legacy_hint_after_give_1 = "这是，你，存在过的证明";

	[LocalizeText("零碎文本")]
	public string term_forerunner_leave_legacy_hint_after_give_2 = "再次见到前，我会好好保管它的";

	[LocalizeText("零碎文本")]
	public string term_forerunner_leave_legacy_hint_choose_share = "你要把遗物分享给其它世界的存在吗？";

	[LocalizeText("零碎文本")]
	public string term_forerunner_leave_legacy_hint_choose_replace_share = "你已经有一个分享遗物了，你要替换它吗？";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_archive_title = "信件档案";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_sender_arg0 = "发件人：{0}";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_this_letter = "举报此信件";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_submitted = "已提交";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_title_submitted = "互动记录";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_official_message = "该留言为预设留言，无法举报";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_unavailable = "当前信件无法举报";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_loading = "读取中…";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_choose_type = "选择问题类型";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_type_offensive = "冒犯或仇恨内容";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_type_harassment = "骚扰或个人信息";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_type_advertising = "广告或无关内容";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_type_other = "其他问题";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_comment_title = "补充说明";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_comment_hint = "请具体描述问题（10～200字）";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_comment_count_arg0 = "{0}/200";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_submit = "提交";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_success = "反馈已记录";

	[LocalizeText("零碎文本")]
	public string term_forerunner_report_success_description = "感谢你的反馈。";

	[LocalizeText("零碎文本")]
	public string term_forerunner_leave_location_at_arg0 = "遗留地点：{0}";

	[LocalizeText("零碎文本")]
	public string term_forerunner_legacy_vote_count_arg0 = "被评价次数：{0}";

	[LocalizeText("零碎文本")]
	public string term_forerunner_legacy_remain_exist_time_arg0 = "剩余存在时间：{0}";

	[LocalizeText("零碎文本")]
	public string term_forerunner_legacy_share_upvote_reward_from_arg0_upvote = "来自其它{0}个世界敬意";

	[LocalizeText("零碎文本")]
	public string term_forerunner_data_load_failed = "先行者数据加载失败";

	[LocalizeText("零碎文本")]
	public string term_contract_try_bonus = "尝试契约";

	[LocalizeText("零碎文本")]
	public string term_contract_finish_bonus = "完成契约";

	[LocalizeText("零碎文本")]
	public string term_contract_difficulty_arg0_bonus = "完成契约难度：{0}";

	[LocalizeText("零碎文本")]
	public string term_contract_success = "契约完成";

	[LocalizeText("零碎文本")]
	public string term_contract_failed = "契约失败";

	[LocalizeText("零碎文本")]
	public string term_contract_bonus_gotten = "（已获得）";

	[LocalizeText("零碎文本")]
	public string term_contract_bonus_achieved = "已达成";

	[LocalizeText("零碎文本")]
	public string term_contract_bonus_not_achieved = "未达成";

	[LocalizeText("零碎文本")]
	public string term_daily_contract_expired_in_arg0_days = "{0} 天后过期";

	[LocalizeText("零碎文本")]
	public string term_got_bonus_count_arg0_per_arg1 = "已获取：{0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_contract_end_date_time_arg0 = "挑战结束时间：{0}";

	[LocalizeText("零碎文本")]
	public string term_contract_remain_time_arg0_to_expire = "挑战剩余时间：{0}";

	[LocalizeText("零碎文本")]
	public string term_finish_difficulty_arg0_to_get_reward = "完成 【{0}】 或更高难度后可获得";

	[LocalizeText("零碎文本")]
	public string term_finish_difficulty_arg0_reward_got_already = "【{0}】难度的奖励已获得";

	[LocalizeText("零碎文本")]
	public string term_worship = "供奉";

	[LocalizeText("零碎文本")]
	public string term_worship_button = "供奉";

	[LocalizeText("零碎文本")]
	public string term_soul_matter_insufficient = "魂祭不足";

	[LocalizeText("零碎文本")]
	public string term_forget_shrine_title = "忘却之约";

	[LocalizeText("零碎文本")]
	public string term_forget_shrine_locked = "继续探索，寻找「{0}」解锁";

	[LocalizeText("零碎文本")]
	public string term_forget_shrine_points_count = "{0}·忘却蜡封：{1}";

	[LocalizeText("零碎文本")]
	public string term_soul_matter_name = "{0}·魂祭";

	[LocalizeText("零碎文本")]
	public string term_matter_insufficient = "{0}数量不足";

	[LocalizeText("零碎文本")]
	public string term_forget_shrine_points = "{0}·忘却蜡封：{1}";

	[LocalizeText("零碎文本")]
	public string term_idol_title_format = "{0}像";

	[LocalizeText("零碎文本")]
	public string term_idol_level_format = "LV:{0}";

	[LocalizeText("零碎文本")]
	public string term_idol_max_level_reached = "已达等级上限";

	[LocalizeText("零碎文本")]
	public string term_card_deck_title_format = "{0}卡组";

	[LocalizeText("零碎文本")]
	public string term_forget_shrine_full = "忘却蜡封不足，无法封印更多卡牌";

	[LocalizeText("零碎文本")]
	public string term_idol_unlock_card_title = "解锁卡牌";

	[LocalizeText("零碎文本")]
	public string term_idol_unlock_blessing_title = "解锁祝福";

	[LocalizeText("零碎文本")]
	public string term_idol_unlock_rune_title = "解锁印记";

	[LocalizeText("零碎文本")]
	public string term_forget_wax_seal_name_with_class = "{0}·忘却蜡封";

	[LocalizeText("零碎文本")]
	public string term_forget_wax_seal_name = "忘却蜡封";

	[LocalizeText("零碎文本")]
	public string term_forget_wax_seal_count = "{0}：{1}";

	[LocalizeText("零碎文本")]
	public string term_forget_wax_seal_reward_description = "赤蜡封住一段未竟的命途，使它暂离仪式的召唤。\n\n<i>可在忘却之约中封印1张{0}卡牌，使其不会在仪式中出现。</i>";

	[LocalizeText("零碎文本")]
	public string term_forget_shrine_seal_cards_title = "将要遗忘的卡牌拖入下方";

	[LocalizeText("零碎文本")]
	public string term_forget_shrine_sealed_cards_hint = "被遗忘的卡牌将不会在仪式奖励中出现";

	[LocalizeText("零碎文本")]
	public string term_forget_shrine_unapplied_changes_confirm = "尚未应用新的封印配置，是否退出？";

	[LocalizeText("零碎文本")]
	public string term_max_level = "已满级";

	[LocalizeText("零碎文本")]
	public string term_insecticide_use_full_hint = "使用次数已满。";

	[LocalizeText("零碎文本")]
	public string term_level_count_arg0 = "等级：{0}";

	[LocalizeText("零碎文本")]
	public string term_exp_count_arg0 = "经验：{0}";

	[LocalizeText("零碎文本")]
	public string term_exp = "经验值";

	[LocalizeText("零碎文本")]
	public string term_arg0_to_arg1 = "{0} → {1}";

	[LocalizeText("零碎文本")]
	public string term_level_arg0_to_level_arg1 = "等级：{0} → 等级：{1}";

	[LocalizeText("零碎文本")]
	public string term_unlock_skin = "解锁皮肤";

	[LocalizeText("零碎文本")]
	public string term_unlocked = "已解锁";

	[LocalizeText("零碎文本")]
	public string term_neighbour_talent_not_unlock_yet = "相邻天赋尚未解锁";

	[LocalizeText("零碎文本")]
	public string term_need_pre_to_unlock = "需要前置解锁";

	[LocalizeText("零碎文本")]
	public string term_unlock = "解锁";

	[LocalizeText("零碎文本")]
	public string term_unlock_item = "解锁{0}";

	[LocalizeText("零碎文本")]
	public string term_unlock_count_arg0 = "解锁数量：{0}";

	[LocalizeText("零碎文本")]
	public string term_not_unlockable = "不可解锁";

	[LocalizeText("零碎文本")]
	public string term_undiscovered = "未发现";

	[LocalizeText("零碎文本")]
	public string term_matter_arg0_is_not_enough = "{0}不足";

	[LocalizeText("零碎文本")]
	public string term_matter_is_not_enough = "资源不足";

	[LocalizeText("零碎文本")]
	public string term_discover_progress_arg0_per_arg1 = "探索进度：{0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_daily_first_win_exp_bonus = "每日首次胜利额外奖励";

	[LocalizeText("零碎文本")]
	public string term_daily_first_3_win_exp_bonus = "每日前3次胜利额外奖励";

	[LocalizeText("零碎文本")]
	public string term_daily_first_end_exp_bonus = "每日首次游戏额外奖励";

	[LocalizeText("零碎文本")]
	public string term_daily_first_5_end_exp_bonus = "每日前5次游戏额外奖励";

	[LocalizeText("零碎文本")]
	public string term_gacha_repeat_token_compensate = "重复获得的项目已兑换";

	[LocalizeText("零碎文本")]
	public string term_character_get_first_soul_reward_arg0_count_arg1 = "首次获得{0}×{1}";

	[LocalizeText("零碎文本")]
	public string term_character_get_repeat_soul_reward_arg0_count_arg1 = "重复获得{0}×{1}";

	[LocalizeText("零碎文本")]
	public string term_gacha_more_arg0_times_gacha_get_pity_character = "再进行 {0} 次探寻后必定获得角色";

	[LocalizeText("零碎文本")]
	public string term_gacha_grand_prize_remian_times_arg0 = "再探寻 <color=#FFFEDD><b>{0}</b></color> 次内";

	[LocalizeText("零碎文本")]
	public string term_gacha_grand_prize_get_character_arg0 = "必得{0}";

	[LocalizeText("零碎文本")]
	public string term_gacha_extra_token_compensate = "额外奖励";

	[LocalizeText("零碎文本")]
	public string term_gacha_gacha_ticket_not_enough_consume_arg0_count_arg1_matter_to_buy_arg2_count_arg3_matter = "{3}不足，是否消耗 {0} {1} 购买 {2} {3}";

	[LocalizeText("零碎文本")]
	public string term_not_enough_matter_arg0_jump_to_purchase = "{0}不足，是否前往购买？";

	[LocalizeText("零碎文本")]
	public string term_current_account_arg0 = "当前账号：{0}";

	[LocalizeText("零碎文本")]
	public string term_current_nickname_arg0 = "昵称：{0}";

	[LocalizeText("零碎文本")]
	public string term_realName_status_disable = "实名制状态：未开启";

	[LocalizeText("零碎文本")]
	public string term_realName_status_visitor = "实名制状态：游客";

	[LocalizeText("零碎文本")]
	public string term_realName_status_children = "实名制状态：未成年";

	[LocalizeText("零碎文本")]
	public string term_realName_status_adult = "实名制状态：成年";

	[LocalizeText("零碎文本")]
	public string term_character = "角色";

	[LocalizeText("零碎文本")]
	public string term_cardskin = "卡牌形态";

	[LocalizeText("零碎文本")]
	public string term_item_skin_entry_locked = "获得可用的卡牌形态后解锁";

	[LocalizeText("零碎文本")]
	public string term_enemy = "敌人";

	[LocalizeText("零碎文本")]
	public string term_other = "其它";

	[LocalizeText("零碎文本")]
	public string term_event = "事件";

	[LocalizeText("零碎文本")]
	public string term_you = "你";

	[LocalizeText("零碎文本")]
	public string term_confirm = "确认";

	[LocalizeText("零碎文本")]
	public string term_cancel = "取消";

	[LocalizeText("零碎文本")]
	public string term_return = "返回";

	[LocalizeText("零碎文本")]
	public string term_game_record_data_load_failed = "游戏记录加载失败";

	[LocalizeText("零碎文本")]
	public string term_test = "测试";

	[LocalizeText("零碎文本")]
	public string term_arg0_crystal = "{0} 魂晶";

	[LocalizeText("零碎文本")]
	public string term_skill = "护符";

	[LocalizeText("零碎文本")]
	public string term_skillWord = "护符词条";

	[LocalizeText("零碎文本")]
	public string term_influence = "祝福";

	[LocalizeText("零碎文本")]
	public string term_item = "卡牌";

	[LocalizeText("零碎文本")]
	public string term_rune = "印记";

	[LocalizeText("零碎文本")]
	public string term_enchant = "附魔";

	[LocalizeText("零碎文本")]
	public string term_keyword = "关键字";

	[LocalizeText("零碎文本")]
	public string term_stance = "架势";

	[LocalizeText("零碎文本")]
	public string term_mastery = "专精";

	[LocalizeText("零碎文本")]
	public string term_worldEvent = "事件";

	[LocalizeText("零碎文本")]
	public string term_get = "获得";

	[LocalizeText("零碎文本")]
	public string term_get_globalItem = "获得物品 ";

	[LocalizeText("零碎文本")]
	public string term_matter = "素材";

	[LocalizeText("零碎文本")]
	public string term_obsession = "执念";

	[LocalizeText("零碎文本")]
	public string term_obsessionDebris = "执念残片";

	[LocalizeText("零碎文本")]
	public string term_click_to_get = "点击获得";

	[LocalizeText("零碎文本")]
	public string term_not_discovered = "未发现";

	[LocalizeText("零碎文本")]
	public string term_not_unlocked = "未解锁";

	[LocalizeText("零碎文本")]
	public string term_not_finished = "未完成";

	[LocalizeText("零碎文本")]
	public string term_finished = "已完成";

	[LocalizeText("零碎文本")]
	public string term_level_arg0 = "等级：{0}";

	[LocalizeText("零碎文本")]
	public string term_default_value = "初始";

	[LocalizeText("零碎文本")]
	public string term_unlock_after = "完成{0}后解锁";

	[LocalizeText("零碎文本")]
	public string term_unlock_after_date = "将于{0}解锁";

	[LocalizeText("零碎文本")]
	public string term_challenge_end = "已结束";

	[LocalizeText("零碎文本")]
	public string term_unlock_after_time_arg0 = "{0} 后解锁";

	[LocalizeText("零碎文本")]
	public string term_unlock_need_season_pass = "需要升格启示解锁";

	[LocalizeText("零碎文本")]
	public string term_wait_for_got = "等待获取";

	[LocalizeText("零碎文本")]
	public string term_coming_soon = "敬请期待";

	[LocalizeText("零碎文本")]
	public string term_skill_info_with_durability_arg0 = "耐久：<color=#FFFF00FF>{0}</color> 点";

	[LocalizeText("零碎文本")]
	public string term_skill_info_with_cast_durability_arg0 = "耐久：<color=#FFFF00FF>{0}</color> 次施放后损坏";

	[LocalizeText("零碎文本")]
	public string term_skill_info_with_battle_durability_arg0 = "耐久：<color=#FFFF00FF>{0}</color> 场战斗后损坏";

	[LocalizeText("零碎文本")]
	public string term_skill_info_with_effect_durability_arg0 = "耐久：<color=#FFFF00FF>{0}</color> 次生效后损坏";

	[LocalizeText("零碎文本")]
	public string term_skill_info_energy_arg0 = "施放：<color=#FFFF00FF>{0}</color> 次/场";

	[LocalizeText("零碎文本")]
	public string term_skill_info_cooldown_arg0 = "冷却：<color=#FFFF00FF>{0}</color> 回合";

	[LocalizeText("零碎文本")]
	public string term_enemyType_normal = "普通";

	[LocalizeText("零碎文本")]
	public string term_enemyType_elite = "精英";

	[LocalizeText("零碎文本")]
	public string term_enemyType_boss = "首领";

	[LocalizeText("零碎文本")]
	public string term_goto_level_arg0_map_arg1 = "“前往 <color=#8FAAAB>{0}<size=80%>-{1}</color>”";

	[LocalizeText("零碎文本")]
	public string term_goto_map_arg0 = "“前往 <color=#8FAAAB>{0}</color>”";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_environmentInfluence_name_arg0_description_arg1 = "{0}：{1}";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_store_item_buy_count_delta_arg0 = "商店中出售卡牌数{0}";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_store_item_sell_cost_delta_arg0 = "商店中移除卡牌价格{0}";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_upgrade_item_cost_delta_arg0 = "锻造间中升级卡牌价格{0}";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_mintage_rune_two_together = "此印记地点会同时附加两个印记";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_mintage_rune_without_slot_limit = "此印记地点可直接重复叠加印记";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_double_crystal_coin_bonus_after_fight = "战斗后获得双倍金币魂晶奖励";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_stronger_enemy = "敌人会变得更加强大";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_extra_bonus_for_bonus_location = "游艺地点地奖励更加丰厚";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_enter_location_to_get_arg0_crystal = "进入地点时获得{0}魂晶";

	[LocalizeText("零碎文本")]
	public string term_location_modifier_lose_percent_arg0_health_enter = "进入地点时时失去{0}%的生命";

	[LocalizeText("零碎文本")]
	public string term_insight_point_effect = "激活效果";

	[LocalizeText("零碎文本")]
	public string term_get_new_skill = "获得新护符";

	[LocalizeText("零碎文本")]
	public string term_draw_skill = "抽取新护符";

	[LocalizeText("零碎文本")]
	public string term_skill_level_arg0 = "星级：{0}";

	[LocalizeText("零碎文本")]
	public string term_skill_upgraded = "已强化到极限";

	[LocalizeText("零碎文本")]
	public string term_skill_upgrade = "强化";

	[LocalizeText("零碎文本")]
	public string term_skill_not_upgraded = "强化主动技能";

	[LocalizeText("零碎文本")]
	public string term_skill_upgrade_arg0_to_arg1 = "升星 {0} -> {1}";

	[LocalizeText("零碎文本")]
	public string term_skillPreset_index_arg0 = "配置{0}";

	[LocalizeText("零碎文本")]
	public string term_skillPreset_c1 = "配置1";

	[LocalizeText("零碎文本")]
	public string term_skillPreset_c2 = "配置2";

	[LocalizeText("零碎文本")]
	public string term_skillPreset_c3 = "配置3";

	[LocalizeText("零碎文本")]
	public string term_skillPreset_c4 = "配置4";

	[LocalizeText("零碎文本")]
	public string term_skill_arg0_durability_decreased = "{0} 的耐久减少了";

	[LocalizeText("零碎文本")]
	public string term_skill_arg0_durability_increased = "{0} 的耐久增加了";

	[LocalizeText("零碎文本")]
	public string term_skill_arg0_removed_for_durability_empty = "因为彻底损坏，{0} 已湮灭";

	[LocalizeText("零碎文本")]
	public string term_swap_skill_arg0_to_arg1 = "替换{0}为{1}";

	[LocalizeText("零碎文本")]
	public string term_activate_skill_arg0 = "激活{0}护符";

	[LocalizeText("零碎文本")]
	public string term_this_skill_is_not_activateable = "此护符无法激活";

	[LocalizeText("零碎文本")]
	public string term_this_skill_activateable_cost_not_enough_coin_need_arg0_count = "护符激活所需金币不足（{0}）";

	[LocalizeText("零碎文本")]
	public string term_this_skill_activateable_cost_not_enough_crystal_need_arg0_count = "护符激活所需魂晶不足（{0}）";

	[LocalizeText("零碎文本")]
	public string term_please_select_to_activate_skill = "选择要激活的护符";

	[LocalizeText("零碎文本")]
	public string term_active = "激活";

	[LocalizeText("零碎文本")]
	public string term_equip = "装备";

	[LocalizeText("零碎文本")]
	public string term_unequip = "卸下";

	[LocalizeText("零碎文本")]
	public string term_replace = "替换";

	[LocalizeText("零碎文本")]
	public string term_please_select_to_swap_skill = "选择要替换的护符";

	[LocalizeText("零碎文本")]
	public string term_select_swap_skill_arg0_with_skill_arg1_ask_confirm = "护符 {0} 将会被 {1} 替换，是否确认";

	[LocalizeText("零碎文本")]
	public string term_delete_skill_with_skillWord_to_pick_select = "分解带有此词条的护符以解锁定向雕刻";

	[LocalizeText("零碎文本")]
	public string term_add_random_skill_word_to_slot = "在该槽位获得一个随机词条";

	[LocalizeText("零碎文本")]
	public string term_add_skill_word_arg0_to_slot = "在该槽位附加{0}";

	[LocalizeText("零碎文本")]
	public string term_word_cant_attach_to_this_skill = "该词条无法附加到该护符上";

	[LocalizeText("零碎文本")]
	public string term_replace_skill_word_arg0_with_random_new_skill_word = "以随机词条替换{0}";

	[LocalizeText("零碎文本")]
	public string term_replace_skill_word_arg0_with_new_skill_word1 = "以{1}替换{0}";

	[LocalizeText("零碎文本")]
	public string term_select_same_skill_word_not_pickable = "词条相同，无法雕刻";

	[LocalizeText("零碎文本")]
	public string term_delete_arg0_count_skill_to_get_below_matter = "分解{0}个护符获得以下资源";

	[LocalizeText("零碎文本")]
	public string term_skillWord_empty_slot = "<color=#939393>空槽位</color>";

	[LocalizeText("零碎文本")]
	public string term_random_skill_as_reward = "随机护符";

	[LocalizeText("零碎文本")]
	public string term_get_skill_word_after_delete_skill_arg0 = "护符分解后，解锁的雕刻词条：{0}";

	[LocalizeText("零碎文本")]
	public string term_without_skillWord_slot = "没有护符槽位";

	[LocalizeText("零碎文本")]
	public string term_skill_info_with_activate_cost_count_arg0_sprite_arg1 = "激活费用：<color=#FFFF00FF>{0}</color> {1}";

	[LocalizeText("零碎文本")]
	public string term_skill_info_need_another_replace_activate = "需要牺牲另一个护符才能激活";

	[LocalizeText("零碎文本")]
	public string term_skill_info_one_cast_only_per_battle = "每场战斗仅可施放一次";

	[LocalizeText("零碎文本")]
	public string term_skill_archtype_name_with_index_arg0 = "{0}型";

	[LocalizeText("零碎文本")]
	public string term_skill_is_character_innate_skill_cant_change = "角色固有护符，不可修改";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_character_innate_skill_initial_activate_arg0_arg1 = "{0}坠入深境时将持有{1}\n<color=#b3b3b3><size=80%><i>角色固有护符将默认初始即激活</i></size></color>";

	[LocalizeText("零碎文本")]
	public string term_character_innate_skill_need_unlock_talent = "<color=#b3b3b3><i>（待解锁该天赋）</i></color>";

	[LocalizeText("零碎文本")]
	public string term_innateskillslot = "固有容器";

	[LocalizeText("零碎文本")]
	public string term_skillslot = "容器";

	[LocalizeText("零碎文本")]
	public string term_withwordskillslot = "本源容器";

	[LocalizeText("零碎文本")]
	public string term_skillslot_add_word_arg0 = "本源容器强化：护符获得{0}";

	[LocalizeText("零碎文本")]
	public string term_mark_favorite_skill_cant_delete = "被收藏的护符无法分解";

	[LocalizeText("零碎文本")]
	public string term_high_quality_skill_cant_delete = "特殊品阶护符无法分解";

	[LocalizeText("零碎文本")]
	public string term_cant_attach_word_to_skill_without_slot = "无法强化没有槽位的护符";

	[LocalizeText("零碎文本")]
	public string term_skill_sort_by_name = "按名称排序";

	[LocalizeText("零碎文本")]
	public string term_skill_sort_by_quality = "按稀有度排序";

	[LocalizeText("零碎文本")]
	public string term_skill_sort_by_favorite = "按收藏排序";

	[LocalizeText("零碎文本")]
	public string term_skill_sort_by_gem_slot = "按镶嵌排序";

	[LocalizeText("零碎文本")]
	public string term_skill_craft = "护符工艺";

	[LocalizeText("零碎文本")]
	public string term_skill_craft_button = "打造";

	[LocalizeText("零碎文本")]
	public string term_skill_add_slot_tab = "打孔";

	[LocalizeText("零碎文本")]
	public string term_skill_mount_gem_tab = "镶嵌";

	[LocalizeText("零碎文本")]
	public string term_skill_breakthrough_tab = "突破";

	[LocalizeText("零碎文本")]
	public string term_skill_delete_tab = "分解";

	[LocalizeText("零碎文本")]
	public string term_sort_by_level = "按等级正序";

	[LocalizeText("零碎文本")]
	public string term_sort_by_level_decrease = "按等级倒序";

	[LocalizeText("零碎文本")]
	public string term_sort_by_type = "按类型排序";

	[LocalizeText("零碎文本")]
	public string term_got_slot = "护符已获得槽位";

	[LocalizeText("零碎文本")]
	public string term_please_select_skill = "请先选择护符";

	[LocalizeText("零碎文本")]
	public string term_no_slot = "当前护符没有宝石孔位";

	[LocalizeText("零碎文本")]
	public string term_guide_return_to_lobby = "返回大厅";

	[LocalizeText("零碎文本")]
	public string term_guide_go_to_amulet = "前往护符";

	[LocalizeText("零碎文本")]
	public string term_guide_select_fearless_accessory = "选择无畏牙饰";

	[LocalizeText("零碎文本")]
	public string term_guide_click_craft = "点击打造";

	[LocalizeText("零碎文本")]
	public string term_guide_click_quick_select = "点击快速选择";

	[LocalizeText("零碎文本")]
	public string term_guide_click_breakthrough = "点击突破";

	[LocalizeText("零碎文本")]
	public string term_please_select_skill_to_delete = "请选择要分解的护符";

	[LocalizeText("零碎文本")]
	public string term_contain_skills_not_deletable = "包含有不可分解的护符";

	[LocalizeText("零碎文本")]
	public string term_without_enough_skill_on_breakthrough_autoSelect = "找不到足够的同名同等级护符以进行突破";

	[LocalizeText("零碎文本")]
	public string term_with_enough_skill_on_breakthrough_autoSelect = "已选择护符突破素材";

	[LocalizeText("零碎文本")]
	public string term_character_class_arg0_sub_skills = "{0}专属护符";

	[LocalizeText("零碎文本")]
	public string term_all_character_class_general_skills = "全职业通用护符";

	[LocalizeText("零碎文本")]
	public string term_new_skill_slot_with_less_slot_count_ask_confirm = "新的护符孔位拥有较少的孔位数，是否确认替换";

	[LocalizeText("零碎文本")]
	public string term_skillgem_insufficient_matter_cost = "打孔资源不足";

	[LocalizeText("零碎文本")]
	public string term_skillgem_slot_state_saved = "已保存 <color=#FFFFFF>当前</color> 槽位状态";

	[LocalizeText("零碎文本")]
	public string term_skillgem_slot_state_keeped = "已保留 <color=#FFF8AB>强化后</color> 槽位状态";

	[LocalizeText("零碎文本")]
	public string term_slot_warning = "旧打孔状态有 {0} 个孔位，因此最多可强化为 {1} 个孔位的状态";

	[LocalizeText("零碎文本")]
	public string term_skillgem_mounting_state_saved = "宝石镶嵌已保存";

	[LocalizeText("零碎文本")]
	public string term_saved = "已保存";

	[LocalizeText("零碎文本")]
	public string term_skillgem_mounting_out_of_range = "宝石超出了范围，无法完成镶嵌";

	[LocalizeText("零碎文本")]
	public string term_skillgem_mounting_overlap = "宝石重叠，无法完成镶嵌";

	[LocalizeText("零碎文本")]
	public string term_need_three_same_level_gem_to_combine_gem = "需要选择3颗同等级宝石才能进行熔炼";

	[LocalizeText("零碎文本")]
	public string term_three_gem_level_not_same_not_combine_able = "选择的3颗宝石等级不同，无法熔炼";

	[LocalizeText("零碎文本")]
	public string term_gem_use_for_skill_upgrade_and_how_to_get = "<color=#FEDC2E>宝石</color>用来给护符进行镶嵌强化\n\n击败首领和精英时有概率掉落，难度越高掉落几率越高";

	[LocalizeText("零碎文本")]
	public string term_enter_location_arg0 = "进入了 {0} 地点";

	[LocalizeText("零碎文本")]
	public string term_trigger_worldEvent_arg0 = "触发 {0} 事件";

	[LocalizeText("零碎文本")]
	public string term_defeat_enemy_arg0 = "击败了 {0}";

	[LocalizeText("零碎文本")]
	public string term_defeated_by_enemy_arg0 = "被 {0} 击败了";

	[LocalizeText("零碎文本")]
	public string term_escape_by_Enemy_arg0 = "{0} 逃跑了";

	[LocalizeText("零碎文本")]
	public string term_mintage_rune_arg0_to_item_arg1 = "将 {0} 附加到 {1}上";

	[LocalizeText("零碎文本")]
	public string term_get_bless_arg0 = "获得祝福 {0}";

	[LocalizeText("零碎文本")]
	public string term_add_item_to_deck_arg0 = "获得卡牌 {0}";

	[LocalizeText("零碎文本")]
	public string term_remove_item_from_deck_arg0 = "移除卡牌 {0}";

	[LocalizeText("零碎文本")]
	public string term_activate_skill_record_arg0 = "激活护符 {0}";

	[LocalizeText("零碎文本")]
	public string term_activate_potential_talent_arg0 = "激活潜能 {0}";

	[LocalizeText("零碎文本")]
	public string term_upgrade_deck_item_arg0 = "升级卡牌 {0}";

	[LocalizeText("零碎文本")]
	public string term_select_skin_mastery = "选择";

	[LocalizeText("零碎文本")]
	public string term_skin_mastery_selected = "已选择";

	[LocalizeText("零碎文本")]
	public string term_apply_mastery = "应用专精";

	[LocalizeText("零碎文本")]
	public string term_unequip_mastery = "卸下专精";

	[LocalizeText("零碎文本")]
	public string term_mastery_applied = "已应用";

	[LocalizeText("零碎文本")]
	public string term_skin_mastery_finish_difficulty_arg0_unlock = "完成{0}难度解锁";

	[LocalizeText("零碎文本")]
	public string term_character_wisdom_arg0_count = "体格：{0}";

	[LocalizeText("零碎文本")]
	public string term_character_willpower_not_enough = "体格不足";

	[LocalizeText("零碎文本")]
	public string term_character_token_upgrade_add_health_arg0 = "初始生命值+{0}";

	[LocalizeText("零碎文本")]
	public string term_character_token_upgrade_add_coin_arg0 = "初始金币+{0}";

	[LocalizeText("零碎文本")]
	public string term_character_token_upgrade_add_crystal_arg0 = "初始魂晶+{0}";

	[LocalizeText("零碎文本")]
	public string term_character_token_upgrade_add_willPower_arg0 = "初始体格+{0}";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_character_obsession_upgrade_unlock_chracter_arg0_mastery_arg1_with_append = "解锁 {0} 的专精：{1}\n专精可以强化角色初始卡组";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_character_obsession_upgrade_unlock_chracter_arg0_add_will_power_arg1_with_append = "{0} 的初始神智+{1}\n神智可以让你击败首领后承受更强大的根源祝福";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_character_obsession_upgrade_with_share_potential_arg0_with_append = "解锁共享潜能 - {0}\n让所有角色都可以在篝火激活该潜能";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_character_obsession_upgrade_unlock_chracter_all_mastery_arg0_with_append = "解锁 {0} 全部的专精：\n<color=#b3b3b3><size=80%><i>专精可以强化角色初始卡组</i></size></color>";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_character_obsession_upgrade_add_character_in_battle_activate_skill_count_by_one = "角色在战斗中可激活使用的护符数量+1";

	[LocalizeText("零碎文本")]
	public string term_talent_share_potential_arg0 = "共享潜能：【{0}】";

	[LocalizeText("零碎文本")]
	public string term_talent_add_skill_preset_slot = "容器强化";

	[LocalizeText("零碎文本")]
	public string term_talent_character_skill_preset_slot_add_one = "角色可用容器+1";

	[LocalizeText("零碎文本")]
	public string term_talent_skill_preset_slot_with_word = "本源容器强化";

	[LocalizeText("零碎文本")]
	public string term_talent_skill_preset_slot_unlock_name_arg0_description_arg1 = "解锁本源容器{0}\n{0}:{1}";

	[LocalizeText("零碎文本")]
	public string term_talent_potential_need_unlock_on_talent_tree = "潜能需在天赋树中解锁";

	[LocalizeText("零碎文本")]
	public string term_talent_potential_is_unlock_need_active_on_healfire = "潜能已解锁，可在旅途中的<color=#fc9d03>篝火</color>处激活";

	[LocalizeText("零碎文本")]
	public string term_talent_tree_level_arg0 = "等级{0}";

	[LocalizeText("零碎文本")]
	public string term_share_potential_character_locked = "角色未解锁";

	[LocalizeText("零碎文本")]
	public string term_share_potential_talent_required = "需要潜能共享天赋";

	[LocalizeText("零碎文本")]
	public string term_season_fixed_hard_challenge_constellation = "高难挑战";

	[LocalizeText("零碎文本")]
	public string term_shuffle_weekly_challenge_constellation = "每周挑战";

	[LocalizeText("零碎文本")]
	public string term_start_game_buff_expire_at_season_end = "赛季结束时过期";

	[LocalizeText("零碎文本")]
	public string term_start_game_buff_expire_in_time_duration = "{0} 后过期";

	[LocalizeText("零碎文本")]
	public string term_start_game_buff_group_name = "仪典";

	[LocalizeText("零碎文本")]
	public string term_account_account_cant_be_empty = "账号不能为空";

	[LocalizeText("零碎文本")]
	public string term_account_password_cant_be_empty = "密码不能为空";

	[LocalizeText("零碎文本")]
	public string term_account_please_agree = "请同意隐私政策";

	[LocalizeText("零碎文本")]
	public string term_guest_account = "游客";

	[LocalizeText("零碎文本")]
	public string term_phone_account = "手机:{0}";

	[LocalizeText("零碎文本")]
	public string term_email_account = "邮箱:{0}";

	[LocalizeText("零碎文本")]
	public string term_account_unknown_error = "未知问题";

	[LocalizeText("零碎文本")]
	public string term_account_account_not_exist = "账号不存在";

	[LocalizeText("零碎文本")]
	public string term_account_account_wrong_password = "账号或密码错误";

	[LocalizeText("零碎文本")]
	public string term_account_agree = "同意";

	[LocalizeText("零碎文本")]
	public string term_account_revoke_agree = "撤回同意";

	[LocalizeText("零碎文本")]
	public string term_account_revoke_agree_warning = "撤回同意并退出游戏";

	[LocalizeText("零碎文本")]
	public string term_account_password_confirm_not_same = "两次输入的密码不一致";

	[LocalizeText("零碎文本")]
	public string term_account_register_success = "注册成功";

	[LocalizeText("零碎文本")]
	public string term_account_register_existed = "账号已存在";

	[LocalizeText("零碎文本")]
	public string term_account_register_accountEmpty = "账号不能为空";

	[LocalizeText("零碎文本")]
	public string term_account_register_nameEmpty = "昵称不能为空";

	[LocalizeText("零碎文本")]
	public string term_account_register_passWord = "密码不能为空";

	[LocalizeText("零碎文本")]
	public string term_account_register_letterAndNumberOnly = "账号只能是字母或者数字";

	[LocalizeText("零碎文本")]
	public string term_account_register_illegalWorlds = "昵称包含违规字符";

	[LocalizeText("零碎文本")]
	public string term_account_register_registering = "正在注册";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_networkError = "网络错误";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_invalidInput = "输入异常";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_notFound = "找不到资源";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_alreadyExists = "已经存在";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_temporaryUnavaliable = "临时不可用";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_closed = "服务关闭";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_networkUnreachable = "网络不可达";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_networkTimeout = "网络请求超时";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_networkUntrusted = "请求被拒绝";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_rateLimited = "服务限流";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_overload = "服务过载";

	[LocalizeText("零碎文本")]
	public string term_network_error_general_internalError = "服务器内部错误";

	[LocalizeText("零碎文本")]
	public string term_network_error_nickname_too_long = "昵称长度过长";

	[LocalizeText("零碎文本")]
	public string term_network_error_nickname_not_allow = "昵称不合法";

	[LocalizeText("零碎文本")]
	public string term_network_error_nickname_not_allow_empty = "昵称不能为空";

	[LocalizeText("零碎文本")]
	public string term_network_error_nickname_not_allow_special_characters = "昵称不能包含特殊字符";

	[LocalizeText("零碎文本")]
	public string term_network_communicating = "通信中";

	[LocalizeText("零碎文本")]
	public string term_network_error_input_text_not_allow = "输入文本存在敏感字符";

	[LocalizeText("零碎文本")]
	public string term_network_error_forerunner_legacy_illegal = "上传遗物不合法";

	[LocalizeText("零碎文本")]
	public string term_journal_content_title_damage_output = "-----伤害输出排名-----";

	[LocalizeText("零碎文本")]
	public string term_journal_content_title_enemy_threat = "-----威胁最高的敌人-----";

	[LocalizeText("零碎文本")]
	public string term_exp_change_record_progress_finish_arg0 = "已完成 {0} 层";

	[LocalizeText("零碎文本")]
	public string term_exp_change_record_finish_arg0_difficulty = "完成 {0} 难度";

	[LocalizeText("零碎文本")]
	public string term_exp_change_record_win_arg0_game_bonus = "每日第{0}局游戏胜利";

	[LocalizeText("零碎文本")]
	public string term_exp_change_record_finish_arg0_game_bonus = "每日第{0}局游戏结束";

	[LocalizeText("零碎文本")]
	public string term_exp_change_record_vip_exp_extra_bonus = "VIP经验增益";

	[LocalizeText("零碎文本")]
	public string term_exp_change_record_settlement_modifier_bonus = "结算祝福增益";

	[LocalizeText("零碎文本")]
	public string term_index_book_title = "图鉴";

	[LocalizeText("零碎文本")]
	public string term_index_book_all_page_collect_ratio_arg0 = "收集率 <size=120%>{0}</size>";

	[LocalizeText("零碎文本")]
	public string term_index_book_get_next_page_layer_reward_with_arg0_count_discover = "再解锁 <color=#F5F0AF>{0}</color> 项可得奖励";

	[LocalizeText("零碎文本")]
	public string term_character_class_arg0_sub_items = "{0}卡组";

	[LocalizeText("零碎文本")]
	public string term_index_other_items = "其他卡牌";

	[LocalizeText("零碎文本")]
	public string term_index_book_discover_progress_arg0 = "{0}收集率";

	[LocalizeText("零碎文本")]
	public string term_index_uncollected = "未收录";

	[LocalizeText("零碎文本")]
	public string term_season_pass_get_extra_reward = "解锁升格启示，获取额外奖励";

	[LocalizeText("零碎文本")]
	public string term_to_unlock_season_pass_hint = "解锁升格启示";

	[LocalizeText("零碎文本")]
	public string term_to_unlock_high_season_pass_hint = "进阶升格启示";

	[LocalizeText("零碎文本")]
	public string term_buy_level_to_get_arg0_level_and_get_sub_rewards = "购买后提升到{0}级，可领取以下奖励";

	[LocalizeText("零碎文本")]
	public string term_high_season_pass_unlock_get_arg0_exp_bonus_arg1_exp_limit_arg2_instant_exp_arg3_level_up = "<color=#E3DCB9><b>升格之旅经验获取</b></color>与<color=#E3DCB9><b>每周上限<b></color>提升<color=#E3DCB9><b>{0}%</b></color></size><br><size=80%><i><color=#899B9B>（当前立即提升 <color=#D9D9D9>{2}</color> 经验，合计 <color=#D9D9D9>{3}</color> 级）</color></i></size>";

	[LocalizeText("零碎文本")]
	public string term_defeat_pvp_defender_count_arg0_per_arg1_max = "击败守卫者 {0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_sum_fight_count_arg0 = "累计交战次数：{0}";

	[LocalizeText("零碎文本")]
	public string term_unknown_pvp_defender = "未知守卫者";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_sum_dealt_damage_arg0 = "累计造成伤害：{0}";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_damage_per_fight_arg0 = "场均伤害：{0}";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_sum_get_matter_arg0_count_arg1 = "累计获得的{0}：{1}";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_remove_remain_time_arg0 = "守卫剩余时间：{0}";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_deck_auto_reset_after_time_arg0 = "{0} 后自动重置";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_avatar_not_select_yet = "尚未选择守卫化身";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_name_not_input_yet = "尚未输入守卫者名字";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_name_not_legal = "守卫者名字不合法";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_need_deck = "守卫者需要配置卡组";

	[LocalizeText("零碎文本")]
	public string term_pvp_defender_deck_not_legal = "守卫者的卡组不合法";

	[LocalizeText("零碎文本")]
	public string term_pvp_mode_unlock_after_time_arg0 = "{0} 后开启";

	[LocalizeText("零碎文本")]
	public string term_pvp_data_load_failed = "PVP数据加载失败";

	[LocalizeText("零碎文本")]
	public string term_pvp_mode_need_drag_rune_comp_to_item = "你需要将印记组件拖动到其他卡牌上";

	[LocalizeText("零碎文本")]
	public string term_pvp_mode_item_count_less_than_minimal_size = "卡牌数量小于所要求的最小值";

	[LocalizeText("零碎文本")]
	public string term_pvp_mode_item_diy_point_greater_than_max = "组件点数超出了所允许的最大值";

	[LocalizeText("零碎文本")]
	public string term_cast_with_description_arg0 = "施放：{0}";

	[LocalizeText("零碎文本")]
	public string term_select_arg0_as_spell_target = "选择{0}的施放目标";

	[LocalizeText("零碎文本")]
	public string term_daily_win_exp_bonus_range = "每日第{0}局胜利经验值加成";

	[LocalizeText("零碎文本")]
	public string term_daily_finish_exp_bonus_range = "每日第{0}局结束经验值加成";

	[LocalizeText("零碎文本")]
	public string term_without_enough_cost_unable_cast = "费用不足，无法施放";

	[LocalizeText("零碎文本")]
	public string term_without_cooldown_unable_cast = "卡牌仍在冷却中，无法施放";

	[LocalizeText("零碎文本")]
	public string term_without_condition_unable_cast = "没有满足施放条件，无法施放";

	[LocalizeText("零碎文本")]
	public string term_item_already_at_head = "已经是队首牌";

	[LocalizeText("零碎文本")]
	public string term_consecutive_dazzling_at_head = "队首已存在连续夺目华彩";

	[LocalizeText("零碎文本")]
	public string term_no_focus_spell = "这张卡没有可以被施放的主动效果";

	[LocalizeText("零碎文本")]
	public string term_choose_item_attach_rune_arg0 = "选择一张未镶嵌卡牌，镶嵌\"{0}\"";

	[LocalizeText("零碎文本")]
	public string term_upgrade_with_crystal_cost_arg0 = "花费{0}魂晶升级";

	[LocalizeText("零碎文本")]
	public string term_upgrade_with_health_arg0 = "付出{0}生命升级";

	[LocalizeText("零碎文本")]
	public string term_set_skin_with_coin_arg0_to_skin_arg1 = "付出{0}金币升变为{1}";

	[LocalizeText("零碎文本")]
	public string term_you_not_yest_unlock_skin_arg1_of_item_arg0 = "你尚未获得 {0} 的 {1} 形态";

	[LocalizeText("零碎文本")]
	public string term_deck_set_skin_able_item_count_arg0 = "可升变卡牌：{0}";

	[LocalizeText("零碎文本")]
	public string term_rune_limit_choose_replace_rune = "护符数量已达到上限，选择需要被替换的护符";

	[LocalizeText("零碎文本")]
	public string term_yinyang_convert_count_arg0_text_arg1_to_text_arg2 = "将{0}{1}转化为{2}";

	[LocalizeText("零碎文本")]
	public string term_yinyang_add = "获得{0}{1}";

	[LocalizeText("零碎文本")]
	public string term_yinyang_consume = "消耗{0}{1}";

	[LocalizeText("零碎文本")]
	public string term_get_this_item_need_price_arg0 = "获取这张卡牌需要支付{0}";

	[LocalizeText("零碎文本")]
	public string term_select_a_defence_card_add_counter_arg0 = "选择一张护甲牌，其点数永久+{0}";

	[LocalizeText("零碎文本")]
	public string term_with_treasure_not_sold_leave_confirm = "还存在未卖出的财宝，是否离开？";

	[LocalizeText("零碎文本")]
	public string term_festival_arg0_check_in_hint = "启程7日签到";

	[LocalizeText("零碎文本")]
	public string term_festival_arg0_check_in_carnival_hint = "狂欢7日签到";

	[LocalizeText("零碎文本")]
	public string term_share_subject = "魂坠深境";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_share_text = "我正在《魂坠深境》中挑战仪式，一起来试试吧！";

	[LocalizeText("零碎文本")]
	public string term_difficulty_arg0_unlocked = "已解锁难度：{0}";

	[LocalizeText("零碎文本")]
	public string term_refresh_confirm = "确认";

	[LocalizeText("零碎文本")]
	public string term_refresh_cancel = "取消";

	[LocalizeText("零碎文本")]
	public string term_refresh = "刷新";

	[LocalizeText("零碎文本")]
	public string term_refresh_with_price_arg0_matter_arg1 = "刷新 {0} {1}";

	[LocalizeText("零碎文本")]
	public string term_root_bonfire_bless_refresh_count_arg0_arg1 = "刷新({0}/{1})";

	[LocalizeText("零碎文本")]
	public string term_remove_item_with_arg0_coin_cost = "<<color=#ff0000>>支付</color> {0} 金币，回收该卡牌";

	[LocalizeText("零碎文本")]
	public string term_remove_item_with_arg0_coin_gain = "<<color=#00ff00>>获得</color> {0} 金币，回收该卡牌";

	[LocalizeText("零碎文本")]
	public string term_destroy_item = "销毁";

	[LocalizeText("零碎文本")]
	public string term_sell_item = "卖出";

	[LocalizeText("零碎文本")]
	public string term_attach_arg0_rune_to_this_card = "为这张卡附加{0}";

	[LocalizeText("零碎文本")]
	public string term_reject_attach_rune_with_already = "这张卡牌已经刻有印记";

	[LocalizeText("零碎文本")]
	public string term_reject_attach_rune_not_accept = "不符合此印记的要求";

	[LocalizeText("零碎文本")]
	public string term_reject_attach_rune_with_unstackable_rune = "所附加的印记不可叠加";

	[LocalizeText("零碎文本")]
	public string term_force_attach_rune_with_arg0_crystal = "花费{0} 魂晶，强制刻印此印记";

	[LocalizeText("零碎文本")]
	public string term_hint_attach_rune_on_curse_confirm = "注意：印记刻印在了诅咒上，是否确认";

	[LocalizeText("零碎文本")]
	public string term_hint_attach_rune_on_oneOff = "注意：印记刻印在了一次性卡牌上，是否确认";

	[LocalizeText("零碎文本")]
	public string term_armor_enchant_arg0 = "护甲强化 +{0}";

	[LocalizeText("零碎文本")]
	public string term_attach_rune_init_text = "神秘的石头造物，凹槽中似乎有什么闪闪发光的东西";

	[LocalizeText("零碎文本")]
	public string term_attach_rune_after_text = "凹槽里的光芒消失了";

	[LocalizeText("零碎文本")]
	public string term_whether_attach_or_not_arg0 = "是否为物品镶嵌{0}";

	[LocalizeText("零碎文本")]
	public string term_unlock_a_skill_slot_with_limit_arg0 = "解锁辅助仪式的护符槽位【{0}】";

	[LocalizeText("零碎文本")]
	public string term_new_unlock_arg0 = "解锁了 {0}";

	[LocalizeText("零碎文本")]
	public string term_apply_all_skin_set_arg0_card_skin = "应用所有 {0} 形态";

	[LocalizeText("零碎文本")]
	public string term_reward_of_finished_contract_arg1_with_difficulty_arg0 = "以 {0} 难度完成 {1} 的奖励";

	[LocalizeText("零碎文本")]
	public string term_reward_unlock_New_CTSO_arg0 = "解锁新内容 {0}";

	[LocalizeText("零碎文本")]
	public string term_reward_unlock_New_Item_arg0 = "解锁新卡牌 {0}";

	[LocalizeText("零碎文本")]
	public string term_reward_unlock_New_Influence_arg0 = "解锁新祝福 {0}";

	[LocalizeText("零碎文本")]
	public string term_new_unlock_New_Rune_arg0 = "解锁新印记 {0}";

	[LocalizeText("零碎文本")]
	public string term_new_unlock_New_WorldEvent_arg0 = "解锁新事件 {0}";

	[LocalizeText("零碎文本")]
	public string term_reward_unlock_New_CTSO_arg1 = "解锁新内容 {0}";

	[LocalizeText("零碎文本")]
	public string term_reward_unlock_New_Item_arg1 = "解锁新卡牌 {0}";

	[LocalizeText("零碎文本")]
	public string term_reward_unlock_New_Influence_arg1 = "解锁新祝福 {0}";

	[LocalizeText("零碎文本")]
	public string term_new_unlock_New_Rune_arg1 = "解锁新印记 {0}";

	[LocalizeText("零碎文本")]
	public string term_new_unlock_New_WorldEvent_arg1 = "解锁新事件 {0}";

	[LocalizeText("零碎文本")]
	public string term_skill_cooldown_unable_cast = "护符正在冷却中";

	[LocalizeText("零碎文本")]
	public string term_skill_unable_cast_not_check_castable = "护符未满足施放条件";

	[LocalizeText("零碎文本")]
	public string term_skill_durability_unable_cast = "可用次数已耗尽";

	[LocalizeText("零碎文本")]
	public string term_skill_need_select_unable_cast = "需要选择目标";

	[LocalizeText("零碎文本")]
	public string term_without_enough_matter = "资源不足";

	[LocalizeText("零碎文本")]
	public string term_levelline_arg0_levelUp_arg1 = "{0} 提升 {1} 级";

	[LocalizeText("零碎文本")]
	public string term_environment_influence_arg0_spawned = "环境 <color=#fcf930>{0}</color> 出现";

	[LocalizeText("零碎文本")]
	public string skin_term_default_cardskin_of_card_arg0 = "{0}-初始形态";

	[LocalizeText("零碎文本")]
	public string skin_batch = "{0}期";

	[LocalizeText("零碎文本")]
	public string term_unset_this_cardskin = "卸下此形态";

	[LocalizeText("零碎文本")]
	public string term_apply_this_cardskin = "应用此形态";

	[LocalizeText("零碎文本")]
	public string term_cardskin_not_got = "未获得此形态";

	[LocalizeText("零碎文本")]
	public string term_cardskin_get_source_title = "获取方式：";

	[LocalizeText("零碎文本")]
	public string term_cardskin_gather_method = "获得方式";

	[LocalizeText("零碎文本")]
	public string term_purchase = "购买";

	[LocalizeText("零碎文本")]
	public string term_cardskin_source_unknown = "未知";

	[LocalizeText("零碎文本")]
	public string term_cardskin_source_dream_mirror_shop = "梦镜商店";

	[LocalizeText("零碎文本")]
	public string term_cardskin_source_ascension_journey = "升格之旅";

	[LocalizeText("零碎文本")]
	public string term_cardskin_source_achievement = "成就";

	[LocalizeText("零碎文本")]
	public string term_cardskin_source_star_challenge = "星图挑战";

	[LocalizeText("零碎文本")]
	public string term_get_from_source_type_unknown = "获得方式未知";

	[LocalizeText("零碎文本")]
	public string term_get_from_source_type_season_levelLine = "从赛季等级中获取";

	[LocalizeText("零碎文本")]
	public string term_get_from_source_type_star_challenge = "从星图挑战中获取";

	[LocalizeText("零碎文本")]
	public string term_get_from_source_type_purchase = "从商店中获取";

	[LocalizeText("零碎文本")]
	public string term_ad_not_finished_to_get_reward = "广告未完成播放，无法获得奖励";

	[LocalizeText("零碎文本")]
	public string term_apply_cardSkin_to_item_arg0_skinset_arg1_to_item_arg2 = "已应用{0}-{1}形态至{2}";

	[LocalizeText("零碎文本")]
	public string term_apply_cardSkin_to_item_arg0_skinset_arg1_to_item_arg3 = "已将 【{0}】 还原至 初始形态";

	[LocalizeText("零碎文本")]
	public string term_all_skin_in_skinset_arg0_is_applied = "已应用所有已解锁{0}形态";

	[LocalizeText("零碎文本")]
	public string term_apply_skin_in_skinset_arg0 = "应用所有 {0} 形态";

	[LocalizeText("零碎文本")]
	public string term_selectGetItemBonus_GenerateCardsBonus = "获得卡牌";

	[LocalizeText("零碎文本")]
	public string term_battleBonusObject_CoinBonus = "获得金币";

	[LocalizeText("零碎文本")]
	public string term_battleBonusObject_CrystalBonus = "获得魂晶";

	[LocalizeText("零碎文本")]
	public string term_battleBonusObject_HealAllBonus = "治疗全部";

	[LocalizeText("零碎文本")]
	public string term_battleBonusObject_HealBonus = "治疗";

	[LocalizeText("零碎文本")]
	public string term_battleBonusObject_AddMaxHealthBonus = "生命值上限";

	[LocalizeText("零碎文本")]
	public string term_battleBonusObject_AddInfluenceBonus = "获得祝福";

	[LocalizeText("零碎文本")]
	public string term_battleBonusObject_SelectIncreaseDefenceBonus = "护甲强化";

	[LocalizeText("零碎文本")]
	public string term_rank_score_source_finish_success = "胜利完成";

	[LocalizeText("零碎文本")]
	public string term_rank_score_source_finish_difficulty_arg0 = "完成{0}难度";

	[LocalizeText("零碎文本")]
	public string term_rank_score_source_finish_time_arg0 = "完成时间:{0}";

	[LocalizeText("零碎文本")]
	public string term_open_test_end_datetime_arg0 = "测试结束日期：{0}";

	[LocalizeText("零碎文本")]
	public string term_open_test_end_remain_time_arg0 = "测试剩余时间：{0}";

	[LocalizeText("零碎文本")]
	public string term_open_test_end_already = "本次测试已结束";

	[LocalizeText("零碎文本")]
	public string term_month_card_remain_arg0_days = "月卡剩余：<color=#CAC49F>{0}</color> 天";

	[LocalizeText("零碎文本")]
	public string term_mail_read_already = "已读";

	[LocalizeText("零碎文本")]
	public string term_mail_not_read_yet = "未读";

	[LocalizeText("零碎文本")]
	public string term_mail_accpet_attachment = "领取";

	[LocalizeText("零碎文本")]
	public string term_mail_attachment_accpeted = "已领取";

	[LocalizeText("零碎文本")]
	public string term_mail_attach_accept_failed = "邮件附件领取失败";

	[LocalizeText("零碎文本")]
	public string term_purchase_title_daily_shuffle = "每日轮换";

	[LocalizeText("零碎文本")]
	public string term_purchase_title_week_shuffle = "每周轮换";

	[LocalizeText("零碎文本")]
	public string term_weekly_token_retention_hint;

	[LocalizeText("零碎文本")]
	public string term_purchase_title_month_shuffle = "每月轮换";

	[LocalizeText("零碎文本")]
	public string term_purchase_title_stable = "常驻商店";

	[LocalizeText("零碎文本")]
	public string term_purchase_title_preset_buy_pack = "购买礼包";

	[LocalizeText("零碎文本")]
	public string term_purchase_title_seaon_pass_arg0 = "{0}";

	[LocalizeText("零碎文本")]
	public string term_purchase_title_realMoney_buy = "充值商城";

	[LocalizeText("零碎文本")]
	public string term_purchase_title_convert_matter = "资源兑换";

	[LocalizeText("零碎文本")]
	public string term_global_item_jump_to_store = "前往商店";

	[LocalizeText("零碎文本")]
	public string term_global_item_jump_to_use = "前往使用";

	[LocalizeText("零碎文本")]
	public string term_purchase_remain_times_arg0 = "限购：{0}";

	[LocalizeText("零碎文本")]
	public string term_purchase_discount_remain_times_arg0 = "限量折扣：{0}";

	[LocalizeText("零碎文本")]
	public string term_purchase_remain_times_day = "今日剩余：{0}";

	[LocalizeText("零碎文本")]
	public string term_purchase_remain_times_week = "本周剩余：{0}";

	[LocalizeText("零碎文本")]
	public string term_purchase_remain_times_month = "本月剩余：{0}";

	[LocalizeText("零碎文本")]
	public string term_purchase_remain_times_endless = "剩余：∞";

	[LocalizeText("零碎文本")]
	public string term_season_trial_soul = "赛季期间，每获取{0}经验可获得{1}×1";

	[LocalizeText("零碎文本")]
	public string term_today = "今日";

	[LocalizeText("零碎文本")]
	public string term_this_week = "本周";

	[LocalizeText("零碎文本")]
	public string term_this_month = "本月";

	[LocalizeText("零碎文本")]
	public string term_next_day = "明日";

	[LocalizeText("零碎文本")]
	public string term_next_week = "下周";

	[LocalizeText("零碎文本")]
	public string term_next_month = "下月";

	[LocalizeText("零碎文本")]
	public string term_purchase_discount_percent_arg0 = "-{0}%";

	[LocalizeText("零碎文本")]
	public string term_purchase_not_purchasable_repeat = "已获得";

	[LocalizeText("零碎文本")]
	public string term_purchase_purchase_count_empty = "已售完";

	[LocalizeText("零碎文本")]
	public string term_purchase_history_order_claimed = "支付成功";

	[LocalizeText("零碎文本")]
	public string term_purchase_history_order_placed = "未支付";

	[LocalizeText("零碎文本")]
	public string term_purchase_history_order_canceled = "支付取消";

	[LocalizeText("零碎文本")]
	public string term_purchase_history_order_refunded = "支付退款";

	[LocalizeText("零碎文本")]
	public string term_not_purchasable_hint = "已不可购买";

	[LocalizeText("零碎文本")]
	public string term_not_purchasable_not_enough_matter_arg0_hint = "{0}不足，无法购买";

	[LocalizeText("零碎文本")]
	public string term_purchase_unavailable = "无法购买";

	[LocalizeText("零碎文本")]
	public string term_free_purchase = "免费";

	[LocalizeText("零碎文本")]
	public string term_sold_out = "已售罄";

	[LocalizeText("零碎文本")]
	public string term_sold_out_short = "售罄";

	[LocalizeText("零碎文本")]
	public string term_sold_out_reset_in_time_arg0 = "{0} 重置";

	[LocalizeText("零碎文本")]
	public string term_card_explore = "每日卡牌盲盒";

	[LocalizeText("零碎文本")]
	public string term_daily_rotating_card_pack = "每日轮换卡包";

	[LocalizeText("零碎文本")]
	public string term_card_pack_arg0 = "{0}";

	[LocalizeText("零碎文本")]
	public string term_buy_card = "当前心选卡牌";

	[LocalizeText("零碎文本")]
	public string term_card_skin_choice = "每日心选卡牌";

	[LocalizeText("零碎文本")]
	public string term_current_card_skin_offer = "当前心选卡牌";

	[LocalizeText("零碎文本")]
	public string term_view = "查看";

	[LocalizeText("零碎文本")]
	public string term_refresh_countdown = "刷新倒计时：";

	[LocalizeText("零碎文本")]
	public string term_limited_time_bundle_countdown_prefix = "结束倒计时：";

	[LocalizeText("零碎文本")]
	public string term_limited_time_bundle_countdown = "限时倒计时：{0}";

	[LocalizeText("零碎文本")]
	public string term_real_money_price_arg0 = "￥{0}";

	[LocalizeText("零碎文本")]
	public string term_matter_count_arg0_arg1 = "{0}×{1}";

	[LocalizeText("零碎文本")]
	public string term_refresh_with_cost_arg0 = "刷新 {0}";

	[LocalizeText("零碎文本")]
	public string term_card_skin_offer_sold_out = "已集齐";

	[LocalizeText("零碎文本")]
	public string term_card_skin_offer_no_refresh_option = "暂无可刷新的卡牌";

	[LocalizeText("零碎文本")]
	public string term_card_skin_offer_all_collected = "已全部集齐";

	[LocalizeText("零碎文本")]
	public string term_refresh_card_skin_offers_confirm_arg0_arg1 = "是否花费 {0} {1} 刷新当前卡牌？";

	[LocalizeText("零碎文本")]
	public string term_loot_matter_append_text = "<size=80%>【材料】</size>";

	[LocalizeText("零碎文本")]
	public string term_obsession_of_character_arg0 = "{0}的执念";

	[LocalizeText("零碎文本")]
	public string term_obsession_of_character_arg0_use_it_to_game_character_description_arg1 = " {0}的执念，借由此物可以使用{0}出战\n{1}";

	[LocalizeText("零碎文本")]
	public string term_item_of_itemskin_arg0 = "卡牌形态：{0}";

	[LocalizeText("零碎文本")]
	public string term_can_convert_item_arg0_into_itemskinset_arg1_with_skinset_description_arg2 = "可以将{0}转换成{1}形态。\n{2}";

	[LocalizeText("零碎文本")]
	public string term_unlock_season_pass = "升格启示";

	[LocalizeText("零碎文本")]
	public string term_unlock_season_pass_with_season_name_arg0 = "{0}升格启示";

	[LocalizeText("零碎文本")]
	public string term_unlock_season_pass_arg0_with_season_description_arg1 = "解锁{0}升格启示\n{1}";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_global_reward_other = "其它奖励";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_random_card_skin_in_skin_set_batch_arg0 = "随机 {0} 卡牌形态";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_draw_random_card_skin_count_arg0 = "随机抽取{0}张卡牌形态";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_get_random_ungot_card_skin_in_skin_set_batch_arg0_with_count_arg0 = "随机获得 {0} 中未获取的卡牌形态 X{1}";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_daily_card_blind_box_description = "从本期轮换系列中，随机获得 1 张尚未拥有的卡牌形态。";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_get_random_card_skin_in_skin_set_batch_arg0_with_count_arg0 = "随机获得 {0} 中的卡牌形态 X{1}";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_item_skin_got_count_arg0_per_all_count_arg1 = "当前已收集 {0}/{1}";

	public Dictionary<Type, GlobalItemStringConfig> globalItemStringConfigs;

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_data_migration_confirm_description = "检测到当前网络可优化，是否立即提升网络体验？";

	[LocalizeText("零碎文本")]
	public string term_data_migration_confirm_button = "立即提升";

	[LocalizeText("零碎文本")]
	public string term_data_migration_cancel_button = "暂不提升";

	[LocalizeText("零碎文本")]
	public string term_data_migration_success = "提升完成";

	[LocalizeText("零碎文本")]
	public string term_data_migration_failed_fallback = "提升失败，请稍后重试";

	[LocalizeText("零碎文本")]
	public string term_real_name_applied = "已实名";

	[LocalizeText("零碎文本")]
	public string term_season_pass_of_arg0 = "{0}升格之旅";

	[LocalizeText("零碎文本")]
	public string term_unlock_constellation_after_finish_count_arg0_challengeStar = "再完成 {0} 个挑战解锁星座";

	[LocalizeText("零碎文本")]
	public string term_constellation_unlocked = "星座已解锁";

	[LocalizeText("零碎文本")]
	public string term_unknown_question_mark = "？？？";

	[LocalizeText("零碎文本")]
	public string term_keep_overflow_item = "▼保留";

	[LocalizeText("零碎文本")]
	public string term_cant_upgrade_anymore = "已经无法升级";

	[LocalizeText("零碎文本")]
	public string term_without_skin_to_set = "卡牌没有可升变的形态";

	[LocalizeText("零碎文本")]
	public string term_item_set_to_skin_arg0_already = "当前已为 {0} 形态";

	[LocalizeText("零碎文本")]
	public string term_skin_arg0_not_unlock_yet = "{0} 形态尚未解锁";

	[LocalizeText("零碎文本")]
	public string term_upgraded = "已升级";

	[LocalizeText("零碎文本")]
	public string term_base_upgrade = "基础升级";

	[LocalizeText("零碎文本")]
	public string term_upgrade_preview = "升级\n预览";

	[LocalizeText("零碎文本")]
	public string term_upgrade_cardskin_arg0_preview = "{0}\n预览";

	[LocalizeText("零碎文本")]
	public string term_game_progress_arg0 = "层数：{0}";

	[LocalizeText("零碎文本")]
	public string term_ranking_score_arg0 = "排行榜分数：{0}";

	[LocalizeText("零碎文本")]
	public string term_item_story = "背景故事";

	[LocalizeText("零碎文本")]
	public string term_map_progress_arg0_per_arg1 = "地图进度：{0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_multiply_X_arg0 = "X{0}";

	[LocalizeText("零碎文本")]
	public string term_arg0_tilde_arg1 = "{0}~{1}";

	[LocalizeText("零碎文本")]
	public string term_cooldown_arg0 = "冷却：{0}";

	[LocalizeText("零碎文本")]
	public string term_coin_arg0_count = "{0} 金币";

	[LocalizeText("零碎文本")]
	public string term_amount_arg0 = "数量：{0}";

	[LocalizeText("零碎文本")]
	public string term_slot_count = "孔位数：{0}";

	[LocalizeText("零碎文本")]
	public string term_start = "开始";

	[LocalizeText("零碎文本")]
	public string term_version_in_arg0 = "版本  {0}";

	[LocalizeText("零碎文本")]
	public string term_amount_arg0_per_ag1 = "数量：{0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_arg0_per_arg1 = "{0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_remain_arg0_days_expire = "剩余{0}日过期";

	[LocalizeText("零碎文本")]
	public string term_remain_time_arg0 = "剩余时间：{0}";

	[LocalizeText("零碎文本")]
	public string term_range_arg0_arg1 = "{0}~{1}";

	[LocalizeText("零碎文本")]
	public string term_arg0_with_count_arg1 = "{0} X{1}";

	[LocalizeText("零碎文本")]
	public string term_game_record_time_arg0 = "游戏时间：{0}";

	[LocalizeText("零碎文本")]
	public string term_game_record_progress_arg0 = "层数：{0}";

	[LocalizeText("零碎文本")]
	public string term_index_unlock_count_arg0_per_arg1 = "已解锁：{0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_index_discover_count_arg0_per_arg1 = "已发现：{0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_character_class_arg0 = "职业：{0}";

	[LocalizeText("用作附魔结构文本，翻译成中文以外的语言时不需要“层”，只需要翻译为{0} {1}即可")]
	public string term_arg0_layer_of_arg1 = "{0}层{1}";

	[LocalizeText("这里是表达不定数张卡牌，中文以外的语言无法表达量词张，可以直接翻译为{0} {1}")]
	public string term_arg0_count_card_arg1 = "{0}张{1}";

	[LocalizeText("零碎文本")]
	public string term_effect_mark_from_arg0_type_arg1_object = "来自{0}<color=#EEEEEE>“{1}”</color>影响";

	[LocalizeText("零碎文本")]
	public string term_effect_mark_from_arg0_source_arg1_description = "{0}：{1}";

	[LocalizeText("零碎文本")]
	public string term_enchant_add_invalid = "（无效）";

	[LocalizeText("零碎文本")]
	public string term_consume_cost_with_arg0 = "消耗：{0}";

	[LocalizeText("零碎文本")]
	public string term_accpet = "接受";

	[LocalizeText("零碎文本")]
	public string term_locked = "未解锁";

	[LocalizeText("零碎文本")]
	public string term_expire_after_arg0_time = "{0}后过期";

	[LocalizeText("零碎文本")]
	public string term_arg0_days = "{0} 天";

	[LocalizeText("零碎文本")]
	public string term_arg0_hours = "{0} 小时";

	[LocalizeText("零碎文本")]
	public string term_arg0_minutes = "{0} 分钟";

	[LocalizeText("零碎文本")]
	public string term_arg0_day = "{0}日";

	[LocalizeText("零碎文本")]
	public string term_arg0_hour = "{0}时";

	[LocalizeText("零碎文本")]
	public string term_arg0_dash_arg1 = "{0}-{1}";

	[LocalizeText("零碎文本")]
	public string term_have_arg0 = "已有：{0}";

	[LocalizeText("零碎文本")]
	public string term_global_reward = "梦境等级";

	[LocalizeText("零碎文本")]
	public string term_gemReward = "宝石奖励";

	[LocalizeText("零碎文本")]
	public string term_seasonReward = "赛季等级线奖励";

	[LocalizeText("零碎文本")]
	public string term_characterReward = "职业等级线奖励";

	[LocalizeText("零碎文本")]
	public string term_contractReward = "游戏契约完成奖励";

	[LocalizeText("零碎文本")]
	public string term_gameReward = "游戏内掉落奖励";

	[LocalizeText("零碎文本")]
	public string term_lack_will = "神智 不足，无法承受始源祝福";

	[LocalizeText("零碎文本")]
	public string term_will_power_over_limit_confirm_arg0 = "将超出<color=#D84E43>{0}</color>点神智，需要支付以上代价";

	[LocalizeText("零碎文本")]
	public string term_will_power_over_limit_confirm = "确定";

	[LocalizeText("零碎文本")]
	public string term_will_power_count_arg0_arg1 = "神智 {0}/{1}";

	[LocalizeText("零碎文本")]
	public string term_difficulty_trial = "难度试炼";

	[LocalizeText("零碎文本")]
	public string term_season_trial = "赛季试炼";

	[LocalizeText("零碎文本")]
	public string term_character_experience = "试玩";

	[LocalizeText("零碎文本")]
	public string term_health = "生命";

	[LocalizeText("零碎文本")]
	public string term_wisdom = "神智";

	[LocalizeText("零碎文本")]
	public string term_coin = "金币";

	[LocalizeText("零碎文本")]
	public string term_crystal = "魂晶";

	[LocalizeText("零碎文本")]
	public string term_draw = "回转";

	[LocalizeText("零碎文本")]
	public string term_slot = "队列上限";

	[LocalizeText("零碎文本")]
	public string term_character_stats_title = "属性";

	[LocalizeText("零碎文本")]
	public string term_character_initial_stats_title = "初始属性";

	[LocalizeText("零碎文本")]
	public string term_questionnaire_answer_to_get_bonus_hint = "回答获得奖励";

	[LocalizeText("零碎文本")]
	public string term_questionnaire_answer_got_bonus_hint = "已获得奖励";

	[LocalizeText("零碎文本")]
	public string term_questionnaire_reward_title = "问卷奖励";

	[LocalizeText("零碎文本")]
	public string term_default_player_name_arg0 = "（默认名称:{0}）";

	[LocalizeText("零碎文本")]
	public string term_gift_code_claim_failed = "礼包码领取失败";

	[LocalizeText("零碎文本")]
	public string term_unlockContent = "全局解锁内容";

	[LocalizeText("零碎文本")]
	public string term_characterUnlockContent = "职业解锁内容";

	[LocalizeText("零碎文本")]
	public string term_soul_sacrifice = "魂祭";

	[LocalizeText("零碎文本")]
	public string term_idol_system_not_unlocked = "偶像系统未解锁";

	[LocalizeText("零碎文本")]
	public string term_DailyAd = "每日赠礼";

	[LocalizeText("零碎文本")]
	public string term_WeeklyAd = "每周赠礼";

	[LocalizeText("零碎文本")]
	public string term_WatchAd = "看广告";

	[LocalizeText("零碎文本")]
	public string term_GetPurchase = "领取";

	[LocalizeText("零碎文本")]
	public string term_VIPSkip_hint = "VIP可跳过广告";

	[LocalizeText("零碎文本")]
	public string term_VIPPurchase = "VIP奖励";

	[LocalizeText("零碎文本")]
	public string term_PermanentVIPPurchase = "永久VIP";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_PermanentVIPPurchaseDescription = "永久VIP";

	[LocalizeText("零碎文本")]
	public string term_random_startGameBuff = "随机仪典";

	[LocalizeText("零碎文本")]
	public string term_get_random_startGameBuff_can_uase_and_within_arg0_days = "获得随机仪典，可在开始游戏时激活使用，{0}天内有效";

	[LocalizeText("零碎文本")]
	public string term_startGameBuff_can_uase_and_within_time_arg0 = "可在开始游戏时激活使用，{0} 内有效";

	[LocalizeText("零碎文本")]
	[TextArea]
	public string term_material = "{0}<color=#797979>/{1}</color>";

	[LocalizeText("零碎文本")]
	public string term_room_lock_need_arg0 = "继续探索，寻找「{0}」解锁";

	[LocalizeText("零碎文本")]
	public string term_room_lock_need_explor = "继续探索，寻找关键道具解锁";

	[LocalizeText("零碎文本")]
	public string term_festival_active_time_range_arg0_to_arg1 = "活动时间：{0}~{1}";

	[LocalizeText("零碎文本")]
	public string term_end_date = "结束时间：M月d日 HH:mm";

	[LocalizeText("零碎文本")]
	public string term_activity_countdown_time = "活动倒计时：{0}天{1}时{2}分";

	[LocalizeText("零碎文本")]
	public string term_activity_unlock_condition = "完成上一个任务后解锁";

	[LocalizeText("零碎文本")]
	public string term_unsell_treasure = "还存在未卖出的财宝，是否离开？";

	[LocalizeText("零碎文本")]
	public string term_unview_bonus = "当前仍有未查看奖励，是否离开？";

	[LocalizeText("零碎文本")]
	public string term_unview_heal_spring = "当前篝火未使用，是否离开？";

	[LocalizeText("零碎文本")]
	public string term_unview_rune = "当前未查看印记，是否离开？";

	[LocalizeText("零碎文本")]
	public string term_unview_influence = "当前未获取始源祝福，是否离开？";

	[LocalizeText("零碎文本")]
	public string term_root_bless_upgrade_title = "始源祝福";

	[LocalizeText("零碎文本")]
	public string term_root_bless_upgrade_description = "选择 1 个始源祝福。";

	[LocalizeText("零碎文本")]
	public string term_root_bless_upgrade_finished_description = "你直面了根须之后的巨物。";

	[LocalizeText("零碎文本")]
	public string term_character_locked = "未持有该角色，无法选中为出战者";

	[LocalizeText("零碎文本")]
	public string term_character_selected = "出战者";

	[LocalizeText("零碎文本")]
	public string term_set_character_selected = "设为出战";

	[LocalizeText("零碎文本")]
	public string term_limited_character_deployment = "可体验";

	[LocalizeText("零碎文本")]
	public string term_set_limited_character_deployment = "体验出战";

	[LocalizeText("零碎文本")]
	public string term_limited_character_selected = "临时出战者";

	[LocalizeText("零碎文本")]
	public string term_unlock_reward_arg0_count_arg1 = "解锁可得{0}×{1}";

	[LocalizeText("零碎文本")]
	public string term_character_locked_need_obsession_to_unlock = "角色未解锁，获得角色的<color=#FFEB92>执念</color>或<color=#FFEB92>执念残片</color>以解锁";

	[LocalizeText("零碎文本")]
	public string term_current_storage = "当前持有: <color=#CFC8BA>{0}</color> 个";

	[LocalizeText("零碎文本")]
	public string term_open_loot_count = "开启 <color=#DAD3B2>{0}</color> 个 {1}";

	[LocalizeText("零碎文本")]
	public string term_open_loot = "使用宝箱";

	[LocalizeText("零碎文本")]
	public string term_upgrade_obsession_level = "强化角色执念等级";

	[LocalizeText("零碎文本")]
	public string term_has = "已有：{0}";

	[LocalizeText("零碎文本")]
	public string term_comsume_loot_get_bonus = "将消耗 <color=#DAD3B2>{0}</color> 个 {1} 获得： <color=#DAD3B2>{2} {3}</color>";

	[LocalizeText("零碎文本")]
	public string term_unlock_obsession = "解锁执念等级 3：<color=#DAD3BD>{0}</color>";

	[LocalizeText("零碎文本")]
	public string term_consume_matter_upgrade_obsession = "消耗 <color=#F0D086>{1}</color> 强化执念等级";

	[LocalizeText("零碎文本")]
	public string term_cannot_upgrade_obsession = "资源不足，无法解锁执念等级{0}： <color=#DAD3BD>{1}</color>";

	[LocalizeText("零碎文本")]
	public string term_upgrade_by_obsession = "通过执念: {0} 解锁";

	[LocalizeText("零碎文本")]
	public string term_unlock_by_obsession_level_arg0_title_arg1 = "执念等级 {0}： <color=#DAD3BD>{1}</color> 解锁";

	[LocalizeText("零碎文本")]
	public string term_obsession_level_arg0_title_arg1 = "执念等级 {0}：<color=#DAD3BD>{1}</color>";

	[LocalizeText("零碎文本")]
	public string term_refresh_remain = "剩余 {0} 天刷新";

	[LocalizeText("零碎文本")]
	public string term_to_unlock_after_arg0 = "将于 {0} 后解锁";

	[LocalizeText("零碎文本")]
	public string term_end_remain1 = "剩余 {0} 结束";

	[LocalizeText("零碎文本")]
	public string term_end = "挑战时间已结束";

	[LocalizeText("零碎文本")]
	public string term_defeat_enemy = "迷途锚定";

	[LocalizeText("零碎文本")]
	public string term_combine_gem_cost_hint = "{0}%熔炼出随机{1}级宝石，{2}%熔炼出随机{3}级宝石";

	[LocalizeText("零碎文本")]
	public string term_combine_gem_cost_hint1 = "熔炼出随机{0}级宝石";

	[LocalizeText("零碎文本")]
	public string term_skill_cannot_upgrade = "护符无法升级";

	[LocalizeText("零碎文本")]
	public string term_not_enough_matter = "素材不足";

	[LocalizeText("零碎文本")]
	public string term_buy_line_level = "已提升升格之旅等级至 <color=#FFF6C8><b>{0}</color></b> 级";

	[LocalizeText("零碎文本")]
	public string term_get_exp = "已获得 <color=#FFF6C8>{0}</color> 升格之旅经验";

	[LocalizeText("零碎文本")]
	public string term_level_upgrade_hint = "升级到{0}级奖励";

	[LocalizeText("零碎文本")]
	public string term_level_upgrades = "提升奖励";

	[LocalizeText("零碎文本")]
	public string term_put_in_three_same_level_gem_to_combine_gem = "投入 3 颗同级宝石，熔炼新宝石";

	[LocalizeText("零碎文本")]
	public string term_put_in_more_gem = "继续放入 LV:{0} 宝石以进行熔炼";

	[LocalizeText("零碎文本")]
	public string term_class_skill = " {0} 专用护符";

	[LocalizeText("零碎文本")]
	public string term_skill_max_level = "该护符已突破至极限";

	[LocalizeText("零碎文本")]
	public string term_two_skill_required = "需要 2 个同样的护符";

	[LocalizeText("零碎文本")]
	public string term_breakthrough_skill = "将护符突破至下一等级";

	[LocalizeText("零碎文本")]
	public string term_gacha_remain_gacha_times_in_period_arg0_count_arg1_limit_arg2 = "{0}剩余探寻次数 {1}/{2}";

	[LocalizeText("零碎文本")]
	public string term_gacha_remain_free_gacha_times_in_period_arg0_count_arg1_limit_arg2 = "{0}剩余免费探寻次数 {1}/{2}";

	[LocalizeText("零碎文本")]
	public string term_draw_Count = "探寻 x{0}";

	[LocalizeText("零碎文本")]
	public string term_remain_refresh_time = "剩余刷新时间：{0}";

	[LocalizeText("零碎文本")]
	public string term_remain_time = "剩余时间：{0}";

	[LocalizeText("零碎文本")]
	public string term_not_enough = "<color=#FFE98A>{0}</color> 不足";

	[LocalizeText("零碎文本")]
	public string term_initialActivate = "初始自动激活";

	[LocalizeText("零碎文本")]
	public string term_random_gem_in_level_arg0 = "随机{0}级宝石";

	[LocalizeText("零碎文本")]
	public string term_random_gem_in_count_arg0_level_arg1 = "{0}个随机{1}级宝石";

	[LocalizeText("零碎文本")]
	public string term_get_random_gem_in_level = "立刻获得随机{0}级宝石 × {1}";

	[LocalizeText("零碎文本")]
	public string term_random_matter_list = "可随机材料列表";

	[LocalizeText("零碎文本")]
	public string term_pick_matter_list = "可选材料列表";

	[LocalizeText("零碎文本")]
	public string term_upgrade_skin_wtih_extra_cost_count_arg0_icon_arg1 = "此形态升变消耗 {0}{1}";

	[LocalizeText("零碎文本")]
	public string term_need_collect_count_arg0_other_batch_skin_to_reduce_cost = "再收集 {0} 个同批次其它形态以降低消耗";

	[LocalizeText("零碎文本")]
	public string term_current_skin_ascension_cost_arg0_icon_arg1 = "当前升变消耗：{0} {1}";

	[LocalizeText("零碎文本")]
	public string term_collect_skin_reduce_ascension_cost_arg0_icon_arg1 = "再收集{0}个形态升变消耗{1}-1";

	[LocalizeText("零碎文本")]
	public string term_skin_ascension_cost_reduction_max = "已达最高降耗";

	[LocalizeText("零碎文本")]
	public string term_replace_skill = "<b>{0}</b> 将被替换为 <b>{1}</b> ，是否确认？";

	[LocalizeText("零碎文本")]
	public string term_bundle = "深境礼包";

	[LocalizeText("零碎文本")]
	public string term_creditCurrency = "异界赠礼";

	[LocalizeText("零碎文本")]
	public string term_item_skin = "卡牌形态";

	[LocalizeText("零碎文本")]
	public string term_real_money_purchase_sub_bundle = "深境之礼";

	[LocalizeText("零碎文本")]
	public string term_real_money_purchase_sub_period = "周期之礼";

	[LocalizeText("零碎文本")]
	public string term_real_money_purchase_sub_itemSkin = "卡牌商店";

	[LocalizeText("零碎文本")]
	public string term_real_money_purchase_sub_credit = "异界赠礼";

	[LocalizeText("零碎文本")]
	public string term_item_reach_limit = "忘却蜡封不足，无法封印更多卡牌";

	public List<ForerunnerPresetMessage> forerunnerPresetMessages;

	public Dictionary<int, LocalizeString> otherTerms;

	public Dictionary<string, LocalizeString> serverTerms;

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_item_draw_count_per_turn_arg0 = "每回合抽卡数：{0}";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_item_table_limit_arg0 = "队列上限：{0}";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_CharacterTrialTips = "【解锁当前职业任意角色时，可获得「魂祭」×1。】\n【提升当前职业任意角色的执念等级时，可获得「魂祭」×1。】";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_SeasonTrialTips = "【使用当前职业获取经验达到一定数量时，可获得「魂祭」×1。】\n【跨赛季时将保留已获得「魂祭」，同时重置「升格之旅」进度。】";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_DifficultyTrialTips = "【使用当前职业首次通关困难至噩梦9层的难度时，可获得对应「魂祭」×1。】\n【首次通关更高可产出难度时，将同时补齐中间可产出难度奖励。】";

	[LocalizeText("零碎文本")]
	public string term_breakthrough_difficulty_arg0_get_soul_arg1 = "突破{0}难度可获得{1}×1";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_journey = "启程";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_class = "职业";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_challenge = "挑战";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_items = "{0}卡组";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_forget_shrine_point = "{0}·忘却蜡封：{1}";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_forget_shrine_limit = "忘却蜡封不足，无法封印更多卡牌";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_purchase_first_double = "首充双倍：";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_purchase_bonus = "赠送：";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_restore_order_purchase_items = "补单物品";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_mainMenuCustomerSupportHint_TapTap = "如遇登录或充值问题，请联系客服QQ：800179233。";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_mainMenuCustomerSupportHint_IOS = "如遇登录或充值问题，请联系客服QQ：800179233。";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_mainMenuCustomerSupportHint_GP = "如遇登录或充值问题，请联系客服info@chillyroom.games。";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_orderListMenuCustomerSupportHint_TapTap = "充值到账可能存在一定时间的延迟，请您稍等10分钟或尝试退出游戏重新登录后再查看是否到账成功。\n如遇到充值长时间不到账等问题，请联系客服QQ：800179233。";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_orderListMenuCustomerSupportHint_IOS = "充值到账可能存在一定时间的延迟，请您稍等10分钟或尝试退出游戏重新登录后再查看是否到账成功。\n如遇到充值长时间不到账等问题，请联系客服QQ：800179233。";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_orderListMenuCustomerSupportHint_GP = "充值到账可能存在一定时间的延迟，请您稍等10分钟或尝试退出游戏重新登录后再查看是否到账成功。\n如遇到充值长时间不到账等问题，请联系客服info@chillyroom.games。";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_resonance = "共鸣";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_current_soul = "当前可供奉魂祭：{0}×{1}";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_got_soul = "已获取魂祭：{0}×{1}";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_repeat_obsession = "重复执念转化";

	[LocalizeText("零碎文本")]
	[TextArea(1, 2)]
	public string term_obsession_turn = "完整执念已转化为对应残片";

	[LocalizeText("零碎文本")]
	public string term_recycle_obsession_debris_summary_arg0_remain_arg1_limit_arg2 = "本次将回收：{0}\u3000今日剩余：{1}/{2}";

	public Color keywordColor = new Color(0.74f, 0.72f, 0.56f);

	public Color linkRuneColor = new Color(0.91f, 1f, 0.58f);

	public Color linkEnchantColor = new Color(0.91f, 1f, 0.58f);

	public Color linkTagColor = new Color(0.91f, 1f, 0.58f);

	public Color linkItemColor = new Color(0.91f, 1f, 0.58f);

	public string[] GetDailyChallengeActivityHelpPages()
	{
		return new string[3] { term_daily_challenge_activity_help_page_1, term_daily_challenge_activity_help_page_2, term_daily_challenge_activity_help_page_3 };
	}

	public string[] GetDailyChallengeRankingHelpPages()
	{
		return new string[3] { term_daily_challenge_ranking_help_page_1, term_daily_challenge_ranking_help_page_2, term_daily_challenge_ranking_help_page_3 };
	}

	public string[] GetDailyChallengeHelpTabTitles()
	{
		return new string[4] { term_daily_challenge_gameplay_help_title, term_daily_challenge_challenge_reward_help_title, term_daily_challenge_ranking_reward_help_title, term_daily_challenge_reward_rules_help_title };
	}

	public string[] GetDailyChallengeHelpTabContents()
	{
		return new string[4]
		{
			string.Join("\n\n", term_daily_challenge_activity_help_page_1, term_daily_challenge_activity_help_page_2),
			term_daily_challenge_activity_help_page_3,
			term_daily_challenge_ranking_help_page_3,
			string.Join("\n\n", term_daily_challenge_ranking_help_page_1, term_daily_challenge_ranking_help_page_2)
		};
	}

	public static string GetServerTerm(string key)
	{
		ScriptableObjectSingleton<TextDataPreset>.instance.serverTerms.TryGetValue(key, out var value);
		return value?.str;
	}

	public string GetThisTimePeriodTerm(ResetTimeType timePeriod)
	{
		return timePeriod switch
		{
			ResetTimeType.Daily => term_today, 
			ResetTimeType.Weekly => term_this_week, 
			ResetTimeType.Monthly => term_this_month, 
			_ => "", 
		};
	}

	public string GetNextTimePeriodTerm(ResetTimeType timePeriod)
	{
		return timePeriod switch
		{
			ResetTimeType.Daily => term_next_day, 
			ResetTimeType.Weekly => term_next_week, 
			ResetTimeType.Monthly => term_next_month, 
			_ => "", 
		};
	}

	public string GetMainMenuCustomerSupportHint()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		ChannelType channelType = CSCommon.channelType;
		if ((int)channelType != 2)
		{
			if ((int)channelType != 3)
			{
				if ((int)channelType == 10)
				{
					return term_mainMenuCustomerSupportHint_TapTap;
				}
				return term_mainMenuCustomerSupportHint_TapTap;
			}
			return term_mainMenuCustomerSupportHint_IOS;
		}
		return term_mainMenuCustomerSupportHint_GP;
	}

	public string GetOrderListMenuCustomerSupportHint()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		ChannelType channelType = CSCommon.channelType;
		if ((int)channelType != 2)
		{
			if ((int)channelType != 3)
			{
				if ((int)channelType == 10)
				{
					return term_orderListMenuCustomerSupportHint_TapTap;
				}
				return term_orderListMenuCustomerSupportHint_TapTap;
			}
			return term_orderListMenuCustomerSupportHint_IOS;
		}
		return term_orderListMenuCustomerSupportHint_GP;
	}

	public string GetQualityLevelText(QualityLevel level, bool withColor = false)
	{
		string text = ScriptableObjectSingleton<VFXDataPreset>.instance.qualityDisplayInfos[level].qualityName;
		if (withColor)
		{
			text = text.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(level));
		}
		return text;
	}

	public static string FormatKeywordDescription(ApplicationStateInfo applicationState, KeywordDef keyword, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(keyword, keyword.keyworkdDescription, applicationState, out generator);
	}

	public static string FormatItemPassiveDescription(ApplicationStateInfo applicationState, Item item, out TextDataGenerator generator, bool isBattleDisplay = false, bool showUncollectedStatus = false)
	{
		string text = item.CurrentItemSkinConfig?.passiveFormatDescription ?? ((item.upgraded && !string.IsNullOrEmpty(item.def.upgradedPassiveFormtDescription)) ? item.def.upgradedPassiveFormtDescription : item.def.passiveFormtDescription);
		if (item != null && item.HasTransformTemplate)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				text = "<mTransformPassiveDesc>";
			}
			else if (!text.Contains("<mTransformPassiveDesc>"))
			{
				text = text.AppendNewLineIfNotEmpty("<mTransformPassiveDesc>");
			}
		}
		return FormatEffectObjectDescription(item, text, applicationState, out generator, isBattleDisplay, showUncollectedStatus);
	}

	public static string FormatItemSpellDescription(ApplicationStateInfo applicationState, Item item, out TextDataGenerator generator, SpellCastInfo spellInfo = null, bool isBattleDisplay = false, bool showUncollectedStatus = false)
	{
		if (item.transformActive)
		{
			return FormatItemTransformSpellDescription(applicationState, item, out generator, spellInfo, isBattleDisplay, showUncollectedStatus);
		}
		string currentSpellFormatDescription = item.CurrentSpellFormatDescription;
		generator = TextDataGenerator.Create(applicationState, item, spellInfo, isBattleDisplay, showUncollectedStatus);
		return generator.Parse(currentSpellFormatDescription ?? "");
	}

	public static string FormatItemOriginalSpellDescription(ApplicationStateInfo applicationState, Item item, out TextDataGenerator generator, SpellCastInfo spellInfo = null, bool isBattleDisplay = false, bool showUncollectedStatus = false)
	{
		string itemOriginalSpellDescriptionRaw = GetItemOriginalSpellDescriptionRaw(item);
		generator = TextDataGenerator.Create(applicationState, item, spellInfo, isBattleDisplay, showUncollectedStatus);
		return generator.Parse(itemOriginalSpellDescriptionRaw ?? "");
	}

	public static string FormatItemTransformSpellDescription(ApplicationStateInfo applicationState, Item item, out TextDataGenerator generator, SpellCastInfo spellInfo = null, bool isBattleDisplay = false, bool showUncollectedStatus = false)
	{
		string itemTransformSpellDescriptionRaw = GetItemTransformSpellDescriptionRaw(item);
		generator = TextDataGenerator.Create(applicationState, item, spellInfo, isBattleDisplay, showUncollectedStatus);
		return generator.Parse(itemTransformSpellDescriptionRaw ?? "");
	}

	public static string GetItemOriginalSpellDescriptionRaw(Item item)
	{
		return item.CurrentSpellFormatDescription;
	}

	public static string GetItemTransformSpellDescriptionRaw(Item item)
	{
		string text = item.runtimeTransformSpellDescription;
		if (text == null)
		{
			ItemDefinition def = item.def;
			if ((object)def == null)
			{
				return null;
			}
			text = def.configuredTransformSpellDescription;
		}
		return text;
	}

	public static string FormatEnchantDescription(ApplicationStateInfo applicationState, Enchant enchant, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(enchant, enchant.def.description, applicationState, out generator);
	}

	public static string FormatEnchantDescription(ApplicationStateInfo applicationState, EnchantDef enchantDef, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(enchantDef, enchantDef.description, applicationState, out generator);
	}

	public static string FormatRuneDescription(ApplicationStateInfo applicationState, Rune rune, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(rune, rune.def.description, applicationState, out generator);
	}

	public static string FormatRuneDescription(ApplicationStateInfo applicationState, RuneDef runeDef, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(runeDef, runeDef.description, applicationState, out generator);
	}

	public static string FormatInfluenceDescription(Influence influence, ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(influence, influence.def.description, applicationState, out generator);
	}

	public static string FormatInfluenceDescription(InfluenceDef influenceDef, ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(influenceDef, influenceDef.description, applicationState, out generator);
	}

	public static string FormatSkillDescription(GameSkill skill, GameStateInfo gameState, bool isBattleDisplay, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(skill, skill.skillDef.description, gameState, out generator, isBattleDisplay);
	}

	public static string FormatSkillDescription(GameSkill skill, ApplicationStateInfo applicationState, bool isBattleDisplay, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(skill, skill.skillDef.description, applicationState, out generator, isBattleDisplay);
	}

	public static string FormatSkillDescription(SkillDef skillDef, ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return FormatSkillDescription(new GameSkill(new ProfileSkill(skillDef, 0)), applicationState, isBattleDisplay: false, out generator);
	}

	public static string FormatSkillWordDefDescription(SkillWordDef skillWordDef, ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(skillWordDef, GetSkillWordRawDescription(skillWordDef), applicationState, out generator);
	}

	public static string FormatSkillWordDescription(SkillWord skillWord, ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(skillWord, GetSkillWordRawDescription(skillWord.def), applicationState, out generator);
	}

	private static string GetSkillWordRawDescription(SkillWordDef skillWordDef)
	{
		if (!skillWordDef.isUnique)
		{
			return skillWordDef.description;
		}
		return "<UniqueSkillWord>" + skillWordDef.description;
	}

	public static string FormatTalentDefEffectDescription(TalentDef talent, ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(talent, talent.effectDescription, applicationState, out generator);
	}

	public static string FormatTalentDefEffectDescription(TalentDef talent, GameStateInfo gameState, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(talent, talent.effectDescription, gameState, out generator);
	}

	public static string FormatContractDescription(Contract contract, ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(contract, contract.contractDef.description, applicationState, out generator);
	}

	public static string FormatStartGameBuffDescription(StartGameBuffDef startGameBuffDef, ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(startGameBuffDef, startGameBuffDef.description, applicationState, out generator);
	}

	public static string FormatStartGameBuffDescription(StartGameBuff startGameBuff, ApplicationStateInfo applicationState, bool withHintAndExpireData, out TextDataGenerator generator)
	{
		string text = FormatEffectObjectDescription(startGameBuff, startGameBuff.def.description, applicationState, out generator);
		if (withHintAndExpireData)
		{
			text = text + Environment.NewLine + ScriptableObjectSingleton<TextDataPreset>.instance.term_startGameBuff_can_uase_and_within_time_arg0.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(startGameBuff.GetTimeToExpire(applicationState)));
		}
		return text;
	}

	public static string FormatPrayDescription(Pray pray, GameStateInfo gs, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(pray, pray.PrayAbilityNode.prayDescription, gs, out generator);
	}

	public static string FormatEnemySecondPhaseAbilityDescription(CharacterAI enemy, GameStateInfo gs, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(enemy, enemy.component.abilityDescription, gs, out generator, isBattleDisplay: true);
	}

	public static string FormatEnemySecondPhaseAbilityDescription(CharacterAIComponent enemy, GameStateInfo gs, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(enemy, enemy.abilityDescription, gs, out generator);
	}

	public static string FormatItemName(GameStateInfo gs, Item item, out TextDataGenerator generator)
	{
		generator = TextDataGenerator.Create(gs, item);
		return generator.GetItemsText(new Item[1] { item });
	}

	public static string FormatEnvironmentInfluenceDescription(GameStateInfo gs, EnvironmentInfluence influence, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(influence, influence.def.description, gs, out generator);
	}

	public static string FormatAchievementDescription(ApplicationStateInfo applicationState, AchievementDef ach, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(ach, ach.rawDescription, applicationState, out generator);
	}

	public static string FormatStanceDescription(ApplicationStateInfo applicationState, Stance stance, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(stance, stance.def.description, applicationState, out generator);
	}

	public static string FormatStanceDescription(GameStateInfo gs, Stance stance, out TextDataGenerator generator)
	{
		return FormatEffectObjectDescription(stance, stance.def.description, gs, out generator);
	}

	public static string FormatEffectObjectDescription<T>(T obj, string rawDescription, ApplicationStateInfo applicationState, out TextDataGenerator generator, bool isBattleDisplay = false, bool showUncollectedStatus = false) where T : IEffectObjectDisplay
	{
		generator = TextDataGenerator.Create(applicationState, obj, isBattleDisplay, showUncollectedStatus);
		return generator.Parse(rawDescription);
	}

	public static string FormatEffectObjectDescription<T>(T obj, string rawDescription, GameStateInfo gameState, out TextDataGenerator generator, bool isBattleDisplay = false, bool showUncollectedStatus = false) where T : IEffectObjectDisplay
	{
		generator = TextDataGenerator.Create(gameState, obj, isBattleDisplay, showUncollectedStatus);
		return generator.Parse(rawDescription);
	}

	public static string FormatEffectObjectDescription<T>(T obj, string rawDescription, GameEvent gameEvent, out TextDataGenerator generator, bool isBattleDisplay = false, bool showUncollectedStatus = false) where T : IEffectObjectDisplay
	{
		generator = TextDataGenerator.Create(gameEvent, obj, isBattleDisplay, showUncollectedStatus);
		return generator.Parse(rawDescription);
	}
}
