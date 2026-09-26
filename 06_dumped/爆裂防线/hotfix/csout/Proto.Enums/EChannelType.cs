using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EChannelType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("Visitor")]
	Visitor = 60000,
	[OriginalName("Zb")]
	Zb = 60001,
	[OriginalName("ZbShop")]
	ZbShop = 60002,
	[OriginalName("ZbRobot")]
	ZbRobot = 60003,
	[OriginalName("Ios")]
	Ios = 70001,
	[OriginalName("TapTap")]
	TapTap = 70002,
	[OriginalName("XiaoMi")]
	XiaoMi = 70003,
	[OriginalName("HuaWei")]
	HuaWei = 70004,
	[OriginalName("Harmony")]
	Harmony = 70005,
	[OriginalName("AliGames")]
	AliGames = 70006,
	[OriginalName("Vivo")]
	Vivo = 70007,
	[OriginalName("Oppo")]
	Oppo = 70008,
	[OriginalName("HYKB")]
	Hykb = 70009,
	[OriginalName("TMGP")]
	Tmgp = 70010,
	[OriginalName("M4399")]
	M4399 = 70011,
	[OriginalName("Bilibili")]
	Bilibili = 70012,
	[OriginalName("DouYin")]
	DouYin = 70013,
	[OriginalName("DYUnion")]
	Dyunion = 70014
}
