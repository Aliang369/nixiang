using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ETaskType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("LoginCount")]
	LoginCount = 101,
	[OriginalName("BarCount")]
	BarCount = 102,
	[OriginalName("BarSpecialCount")]
	BarSpecialCount = 103,
	[OriginalName("ShopBuyCount")]
	ShopBuyCount = 104,
	[OriginalName("WorldEventCount")]
	WorldEventCount = 105,
	[OriginalName("LegionDonateCount")]
	LegionDonateCount = 107,
	[OriginalName("FriendsActionGiveCount")]
	FriendsActionGiveCount = 108,
	[OriginalName("MonsterKillCount")]
	MonsterKillCount = 109,
	[OriginalName("GemComposeCount")]
	GemComposeCount = 110,
	[OriginalName("TiliCostCount")]
	TiliCostCount = 111,
	[OriginalName("EquipSubLevelUpgradeCount")]
	EquipSubLevelUpgradeCount = 112,
	[OriginalName("HomeRoomCreate")]
	HomeRoomCreate = 113,
	[OriginalName("MainQuestPass")]
	MainQuestPass = 114,
	[OriginalName("TeamAttack")]
	TeamAttack = 115,
	[OriginalName("GemCount")]
	GemCount = 116,
	[OriginalName("EquipCount")]
	EquipCount = 117,
	[OriginalName("GemQualityUseCount")]
	GemQualityUseCount = 118,
	[OriginalName("EquipQualityUseCount")]
	EquipQualityUseCount = 119,
	[OriginalName("HeroCount")]
	HeroCount = 120,
	[OriginalName("HeroQualityUseCount")]
	HeroQualityUseCount = 121,
	[OriginalName("EquipLevelUseCount")]
	EquipLevelUseCount = 122,
	[OriginalName("EquipSubLevelUseCount")]
	EquipSubLevelUseCount = 123,
	[OriginalName("HomeFloorUnlock")]
	HomeFloorUnlock = 124,
	[OriginalName("HomeWorkerCount")]
	HomeWorkerCount = 125,
	[OriginalName("EliteQuestFightCount")]
	EliteQuestFightCount = 127,
	[OriginalName("WorldGrideExplored")]
	WorldGrideExplored = 130,
	[OriginalName("WorldBuildingResCount")]
	WorldBuildingResCount = 131,
	[OriginalName("WorldBossFightCount")]
	WorldBossFightCount = 133,
	[OriginalName("WorldLairFightCount")]
	WorldLairFightCount = 134,
	[OriginalName("HeroTrialFightCount")]
	HeroTrialFightCount = 136,
	[OriginalName("LegionBossKillCount")]
	LegionBossKillCount = 138,
	[OriginalName("NewHero")]
	NewHero = 140,
	[OriginalName("NewHeroQuality")]
	NewHeroQuality = 141,
	[OriginalName("NewGemQuality")]
	NewGemQuality = 142,
	[OriginalName("SRarityHeroCount")]
	SrarityHeroCount = 143,
	[OriginalName("PeerageUpgrade")]
	PeerageUpgrade = 144,
	[OriginalName("RoleNameChange")]
	RoleNameChange = 145,
	[OriginalName("ExchangeGoldCount")]
	ExchangeGoldCount = 146,
	[OriginalName("MechaActive")]
	MechaActive = 147,
	[OriginalName("WallUpgrade3Star")]
	WallUpgrade3Star = 148,
	[OriginalName("SuitActiveCount")]
	SuitActiveCount = 149,
	[OriginalName("GemCastCount")]
	GemCastCount = 150,
	[OriginalName("GeneLevelSet")]
	GeneLevelSet = 151,
	[OriginalName("GeneSlotFiveHole")]
	GeneSlotFiveHole = 152,
	[OriginalName("IdleFastGainCount")]
	IdleFastGainCount = 153,
	[OriginalName("ArenaFightCount")]
	ArenaFightCount = 155,
	[OriginalName("EndlessHordeFightCount")]
	EndlessHordeFightCount = 156,
	[OriginalName("LegendaryFightCount")]
	LegendaryFightCount = 157,
	[OriginalName("ElementTrialFightCount")]
	ElementTrialFightCount = 158,
	[OriginalName("BarTreasureAndPlusCount")]
	BarTreasureAndPlusCount = 159,
	[OriginalName("MineWarCostCount")]
	MineWarCostCount = 160,
	[OriginalName("MainQuestFailCount")]
	MainQuestFailCount = 161,
	[OriginalName("DiamondCostCount")]
	DiamondCostCount = 162,
	[OriginalName("VoucherRechargeCount")]
	VoucherRechargeCount = 164,
	[OriginalName("VoucherRechargeSum")]
	VoucherRechargeSum = 165,
	[OriginalName("BossSubViewCount")]
	BossSubViewCount = 166,
	[OriginalName("BossSubFightCount")]
	BossSubFightCount = 167,
	[OriginalName("BossSubSkillLevel")]
	BossSubSkillLevel = 168,
	[OriginalName("BossSubScoreCount")]
	BossSubScoreCount = 169,
	[OriginalName("BossSubRouletteCount")]
	BossSubRouletteCount = 170,
	[OriginalName("BossSubPass")]
	BossSubPass = 171,
	[OriginalName("TruckStartCount")]
	TruckStartCount = 172,
	[OriginalName("TruckRobCount")]
	TruckRobCount = 173,
	[OriginalName("TrainTransportCount")]
	TrainTransportCount = 174,
	[OriginalName("TrainRobCount")]
	TrainRobCount = 175,
	[OriginalName("CrossLadderFightCount")]
	CrossLadderFightCount = 176,
	[OriginalName("FacilityCreateCount")]
	FacilityCreateCount = 177,
	[OriginalName("FacilityJoinExploreCount")]
	FacilityJoinExploreCount = 178,
	[OriginalName("AdvertisementWatchCount")]
	AdvertisementWatchCount = 180,
	[OriginalName("ExchangeTiliCount")]
	ExchangeTiliCount = 181,
	[OriginalName("ShopDiamondCostCount")]
	ShopDiamondCostCount = 182,
	[OriginalName("WallUpgradeLevelCount")]
	WallUpgradeLevelCount = 183,
	[OriginalName("HeroFollowUpgradeCount")]
	HeroFollowUpgradeCount = 184,
	[OriginalName("HeroAwakenCount")]
	HeroAwakenCount = 185,
	[OriginalName("RechargeCount")]
	RechargeCount = 186,
	[OriginalName("SeasonOccupyCity")]
	SeasonOccupyCity = 187,
	[OriginalName("SeasonKillGuard")]
	SeasonKillGuard = 188,
	[OriginalName("SeasonKillPlayer")]
	SeasonKillPlayer = 189,
	[OriginalName("SeasonTechUpgrade")]
	SeasonTechUpgrade = 190
}
