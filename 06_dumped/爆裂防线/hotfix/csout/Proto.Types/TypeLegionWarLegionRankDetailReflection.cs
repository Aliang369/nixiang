using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarLegionRankDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarLegionRankDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkxlZ2lvbldhckxlZ2lvblJhbmtEZXRhaWwucHJvdG8SBXR5cGVz" + "Ghl0eXBlLkxlZ2lvbkZsYWdJbmZvLnByb3RvGhN0eXBlLlJhbmtEYXRhLnBy" + "b3RvIm0KGUxlZ2lvbldhckxlZ2lvblJhbmtEZXRhaWwSIQoIUmFua0RhdGEY" + "ASABKAsyDy50eXBlcy5SYW5rRGF0YRItCg5MZWdpb25GbGFnSW5mbxgCIAEo" + "CzIVLnR5cGVzLkxlZ2lvbkZsYWdJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLegionFlagInfoReflection.Descriptor,
			TypeRankDataReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarLegionRankDetail), (MessageParser)(object)LegionWarLegionRankDetail.Parser, new string[2] { "RankData", "LegionFlagInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
