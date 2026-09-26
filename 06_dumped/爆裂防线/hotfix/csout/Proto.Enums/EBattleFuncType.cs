using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EBattleFuncType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("MainQuest")]
	MainQuest = 1,
	[OriginalName("EliteQuest")]
	EliteQuest = 2,
	[OriginalName("DungeonEasyFight")]
	DungeonEasyFight = 3,
	[OriginalName("DungeonRiotFight")]
	DungeonRiotFight = 4,
	[OriginalName("DungeonBossFight")]
	DungeonBossFight = 5,
	[OriginalName("LegionBoss")]
	LegionBoss = 7,
	[OriginalName("WorldBoss")]
	WorldBoss = 8,
	[OriginalName("WorldRescueDay")]
	WorldRescueDay = 9,
	[OriginalName("WorldRescueNight")]
	WorldRescueNight = 10,
	[OriginalName("WorldEventFightDay")]
	WorldEventFightDay = 11,
	[OriginalName("WorldEventFightNight")]
	WorldEventFightNight = 12,
	[OriginalName("WorldLair")]
	WorldLair = 13,
	[OriginalName("WorldLairHelp")]
	WorldLairHelp = 14,
	[OriginalName("EndlessHorde")]
	EndlessHorde = 15,
	[OriginalName("Legendary")]
	Legendary = 16,
	[OriginalName("HeroTrialQiang")]
	HeroTrialQiang = 21,
	[OriginalName("HeroTrialPao")]
	HeroTrialPao = 22,
	[OriginalName("HeroTrialJiguang")]
	HeroTrialJiguang = 23,
	[OriginalName("HeroTrialBingdan")]
	HeroTrialBingdan = 24,
	[OriginalName("HeroTrialRanshaodan")]
	HeroTrialRanshaodan = 25,
	[OriginalName("HeroTrialHongzha")]
	HeroTrialHongzha = 26,
	[OriginalName("HeroTrialDianci")]
	HeroTrialDianci = 27,
	[OriginalName("HeroTrialWurenji")]
	HeroTrialWurenji = 28,
	[OriginalName("HeroTrialMotuo")]
	HeroTrialMotuo = 29,
	[OriginalName("HeroTrialBingbao")]
	HeroTrialBingbao = 30,
	[OriginalName("HeroTrialShexian")]
	HeroTrialShexian = 31,
	[OriginalName("HeroTrialDianqiu")]
	HeroTrialDianqiu = 32,
	[OriginalName("HeroTrialLongjuan")]
	HeroTrialLongjuan = 33,
	[OriginalName("HeroTrialFengren")]
	HeroTrialFengren = 34,
	[OriginalName("ElementTrialElec")]
	ElementTrialElec = 35,
	[OriginalName("ElementTrialFire")]
	ElementTrialFire = 36,
	[OriginalName("ElementTrialIce")]
	ElementTrialIce = 37,
	[OriginalName("ElementTrialPhy")]
	ElementTrialPhy = 38,
	[OriginalName("ElementTrialEnergy")]
	ElementTrialEnergy = 39,
	[OriginalName("ElementTrialWind")]
	ElementTrialWind = 40,
	[OriginalName("GuardVerify")]
	GuardVerify = 41,
	[OriginalName("Arena")]
	Arena = 42,
	[OriginalName("CrossLadderMatch")]
	CrossLadderMatch = 43,
	[OriginalName("CrossLadderUpgrade")]
	CrossLadderUpgrade = 44,
	[OriginalName("MineWar")]
	MineWar = 45,
	[OriginalName("CrossMine")]
	CrossMine = 46,
	[OriginalName("PeakArena")]
	PeakArena = 47,
	[OriginalName("Truck")]
	Truck = 48,
	[OriginalName("LegionWar")]
	LegionWar = 49,
	[OriginalName("Facility")]
	Facility = 50,
	[OriginalName("BossSubject")]
	BossSubject = 51,
	[OriginalName("WallSubject")]
	WallSubject = 52,
	[OriginalName("Train")]
	Train = 53,
	[OriginalName("Season")]
	Season = 54,
	[OriginalName("MechaSubject")]
	MechaSubject = 55,
	[OriginalName("PlayerPk")]
	PlayerPk = 56
}
