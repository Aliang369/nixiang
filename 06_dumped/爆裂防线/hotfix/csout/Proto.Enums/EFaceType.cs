using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EFaceType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Hairstyle")]
	Hairstyle,
	[OriginalName("Eyebrow")]
	Eyebrow,
	[OriginalName("Eye")]
	Eye,
	[OriginalName("Beard")]
	Beard,
	[OriginalName("Mouth")]
	Mouth
}
