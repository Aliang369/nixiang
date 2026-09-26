using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EMoneyType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("Gold")]
	Gold = 1,
	[OriginalName("Diamond")]
	Diamond = 2,
	[OriginalName("Voucher")]
	Voucher = 3,
	[OriginalName("Tili")]
	Tili = 4,
	[OriginalName("RoleExp")]
	RoleExp = 5,
	[OriginalName("VipExp")]
	VipExp = 6,
	[OriginalName("BarNormal")]
	BarNormal = 7,
	[OriginalName("BarSpecial")]
	BarSpecial = 8,
	[OriginalName("HeroSoul")]
	HeroSoul = 11,
	[OriginalName("EquipSoul")]
	EquipSoul = 12,
	[OriginalName("Friendly")]
	Friendly = 13,
	[OriginalName("Honor")]
	Honor = 14,
	[OriginalName("Tooth")]
	Tooth = 15,
	[OriginalName("Legendary")]
	Legendary = 16,
	[OriginalName("ArenaCoin")]
	ArenaCoin = 17,
	[OriginalName("GeneCoin")]
	GeneCoin = 19,
	[OriginalName("BarTreasure")]
	BarTreasure = 20,
	[OriginalName("BarTreasurePlus")]
	BarTreasurePlus = 21,
	[OriginalName("LimitTimeHero")]
	LimitTimeHero = 31,
	[OriginalName("LimitTimeCatcher")]
	LimitTimeCatcher = 33,
	[OriginalName("LimitTimeRoulette")]
	LimitTimeRoulette = 34,
	[OriginalName("CrystalBall")]
	CrystalBall = 35,
	[OriginalName("FacilityChestKey")]
	FacilityChestKey = 36,
	[OriginalName("BossSubject")]
	BossSubject = 37,
	[OriginalName("TrainTicket")]
	TrainTicket = 38,
	[OriginalName("TacticBar")]
	TacticBar = 39,
	[OriginalName("TacticCoin")]
	TacticCoin = 40,
	[OriginalName("SeasonCoin")]
	SeasonCoin = 41
}
