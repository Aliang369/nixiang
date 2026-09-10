using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class PlayerProfileData
{
	[JsonProperty("nickName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string NickName { get; set; }

	[JsonProperty("vip", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool Vip { get; set; }

	[JsonProperty("dailyObsessionDebrisRecycleCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int DailyObsessionDebrisRecycleCount { get; set; }

	[JsonProperty("matterInfos", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<MatterInfo> MatterInfos { get; set; }

	[JsonProperty("globalVariables", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Dictionary<string, int> GlobalVariables { get; set; }

	[JsonProperty("storyPointFinishStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Dictionary<string, StoryPointState> StoryPointFinishStates { get; set; }

	[JsonProperty("finishedMaxDiffuclty", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DifficultyLevel FinishedMaxDiffuclty { get; set; }

	[JsonProperty("globalLevelLineInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public LevelLineInfo GlobalLevelLineInfo { get; set; }

	[JsonProperty("seasonInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public SeasonInfo SeasonInfo { get; set; }

	[JsonProperty("gotQuestExpThisWeek", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int GotQuestExpThisWeek { get; set; }

	[JsonProperty("cardSkinUnlockLevels", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Dictionary<string, int> CardSkinUnlockLevels { get; set; }

	[JsonProperty("characterClassStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<CharacterClassState> CharacterClassStates { get; set; }

	[JsonProperty("idolStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<IdolState> IdolStates { get; set; }

	[JsonProperty("characterStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<PlayerCharacterState> CharacterStates { get; set; }

	[JsonProperty("completedCharacterTrialIds", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> CompletedCharacterTrialIds { get; set; }

	[JsonProperty("clavisStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<ClavisState> ClavisStates { get; set; }

	[JsonProperty("skillPool", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<ProfileSkill> SkillPool { get; set; }

	[JsonProperty("favoriteSkillIds", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> FavoriteSkillIds { get; set; }

	[JsonProperty("unlockedSkillWords", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> UnlockedSkillWords { get; set; }

	[JsonProperty("currentStartGameBuffs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<StartGameBuff> CurrentStartGameBuffs { get; set; }

	[JsonProperty("unLockedEntries", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> UnLockedEntries { get; set; }

	[JsonProperty("gotObject", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> GotObject { get; set; }

	[JsonProperty("activePurchaseGroups", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> ActivePurchaseGroups { get; set; }

	[JsonProperty("purchaseRecords", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Dictionary<string, List<PurchaseRecord>> PurchaseRecords { get; set; }

	[JsonProperty("weekRoutineQuestStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<RoutineQuestState> WeekRoutineQuestStates { get; set; }

	[JsonProperty("dailyRoutineQuestStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<RoutineQuestState> DailyRoutineQuestStates { get; set; }

	[JsonProperty("seasonRoutineQeustStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<RoutineQuestState> SeasonRoutineQeustStates { get; set; }

	[JsonProperty("achievementStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<AchievementState> AchievementStates { get; set; }

	[JsonProperty("fortressNpcStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<FortressNPCState> FortressNpcStates { get; set; }

	[JsonProperty("localForerunnerLegacy", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public ForerunnerLegacy LocalForerunnerLegacy { get; set; }

	[JsonProperty("sharedForerunnerLegacy", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public ForerunnerLegacy SharedForerunnerLegacy { get; set; }

	[JsonProperty("lastStartGameInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public StartGameInfo LastStartGameInfo { get; set; }

	[JsonProperty("guideInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GuideInfo GuideInfo { get; set; }

	[JsonProperty("statisticsInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public PlayerStatisticsInfo StatisticsInfo { get; set; }

	[JsonProperty("contractStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<ContractState> ContractStates { get; set; }

	[JsonProperty("currentWeekShuffleContracts", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> CurrentWeekShuffleContracts { get; set; }

	[JsonProperty("gachaPoolStates", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GachaPoolState> GachaPoolStates { get; set; }

	[JsonProperty("testQuestionnaireBonusGet", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool TestQuestionnaireBonusGet { get; set; }

	[JsonProperty("festivalCheckInState", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public FestivalCheckInState FestivalCheckInState { get; set; }

	[JsonProperty("newbieFestivalInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GetNewbieFestivalInfoResponse NewbieFestivalInfo { get; set; }

	[JsonProperty("replaceDailyQuestCountToday", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ReplaceDailyQuestCountToday { get; set; }

	[JsonProperty("monthlyCardExpireDayPeriod", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int MonthlyCardExpireDayPeriod { get; set; }

	[JsonProperty("cheatScore", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int CheatScore { get; set; }

	[JsonProperty("skillGems", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Dictionary<string, int> SkillGems { get; set; }

	[JsonProperty("pvpModeInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public ProfiledDataPVPModeInfo PvpModeInfo { get; set; }

	[JsonProperty("limitedTimeBundleInfos")]
	[Preserve]
	public List<LimitedTimeBundleInfo> LimitedTimeBundleInfos { get; set; } = new List<LimitedTimeBundleInfo>();
}
