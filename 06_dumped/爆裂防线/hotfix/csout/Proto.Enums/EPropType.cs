using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EPropType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("Material")]
	Material = 1,
	[OriginalName("Ore")]
	Ore = 2,
	[OriginalName("Pickaxe")]
	Pickaxe = 3,
	[OriginalName("Drill")]
	Drill = 4,
	[OriginalName("Bomb")]
	Bomb = 5,
	[OriginalName("Suit")]
	Suit = 8,
	[OriginalName("Gun")]
	Gun = 9,
	[OriginalName("Head")]
	Head = 10,
	[OriginalName("HeadFrame")]
	HeadFrame = 11,
	[OriginalName("Mecha")]
	Mecha = 12,
	[OriginalName("Wall")]
	Wall = 13,
	[OriginalName("Foothold")]
	Foothold = 14,
	[OriginalName("VoucherCoupon")]
	VoucherCoupon = 15,
	[OriginalName("Worker")]
	Worker = 16,
	[OriginalName("SuitConvert")]
	SuitConvert = 17,
	[OriginalName("GunConvert")]
	GunConvert = 18,
	[OriginalName("MechaConvert")]
	MechaConvert = 19,
	[OriginalName("WallConvert")]
	WallConvert = 20,
	[OriginalName("FootholdConvert")]
	FootholdConvert = 21,
	[OriginalName("Treasure")]
	Treasure = 22,
	[OriginalName("ServantEggs")]
	ServantEggs = 23,
	[OriginalName("ServantConvert")]
	ServantConvert = 24
}
