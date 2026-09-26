using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EAdvertisementType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("ModRefresh")]
	ModRefresh,
	[OriginalName("ModWheel")]
	ModWheel,
	[OriginalName("MainAwardDouble")]
	MainAwardDouble,
	[OriginalName("GiftAward")]
	GiftAward,
	[OriginalName("LegionDonate")]
	LegionDonate,
	[OriginalName("BarNormal")]
	BarNormal,
	[OriginalName("BarSpecial")]
	BarSpecial,
	[OriginalName("LimitTimeRoulette")]
	LimitTimeRoulette,
	[OriginalName("LimitTimeTreasure")]
	LimitTimeTreasure,
	[OriginalName("LimitTimeVendor")]
	LimitTimeVendor,
	[OriginalName("ChapterOneBar")]
	ChapterOneBar,
	[OriginalName("ChapterTwoBar")]
	ChapterTwoBar,
	[OriginalName("ChapterThreeBar")]
	ChapterThreeBar,
	[OriginalName("BossSubRoulette")]
	BossSubRoulette,
	[OriginalName("AdsDirect")]
	AdsDirect,
	[OriginalName("AdsGift")]
	AdsGift,
	[OriginalName("AdsRoulette")]
	AdsRoulette,
	[OriginalName("AdsSign")]
	AdsSign,
	[OriginalName("AdsZhanLing")]
	AdsZhanLing,
	[OriginalName("WallSubjectRoulette")]
	WallSubjectRoulette,
	[OriginalName("MechaSubjectRoulette")]
	MechaSubjectRoulette
}
