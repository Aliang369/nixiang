using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EResourceType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("Money")]
	Money = 1,
	[OriginalName("Prop")]
	Prop = 2,
	[OriginalName("Item")]
	Item = 3,
	[OriginalName("Equip")]
	Equip = 4,
	[OriginalName("Hero")]
	Hero = 5,
	[OriginalName("HeroFrag")]
	HeroFrag = 7,
	[OriginalName("Gem")]
	Gem = 8,
	[OriginalName("Box")]
	Box = 10,
	[OriginalName("Gene")]
	Gene = 11,
	[OriginalName("Tactic")]
	Tactic = 12,
	[OriginalName("TacticFrag")]
	TacticFrag = 13
}
