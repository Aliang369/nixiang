using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarPlayerRankDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarPlayerRankDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkxlZ2lvbldhclBsYXllclJhbmtEZXRhaWwucHJvdG8SBXR5cGVz" + "Ghl0eXBlLkxlZ2lvbkZsYWdJbmZvLnByb3RvGhN0eXBlLlJhbmtEYXRhLnBy" + "b3RvGhd0eXBlLlJvbGVCYXNlSW5mby5wcm90byKYAQoZTGVnaW9uV2FyUGxh" + "eWVyUmFua0RldGFpbBIhCghSYW5rRGF0YRgBIAEoCzIPLnR5cGVzLlJhbmtE" + "YXRhEikKDFJvbGVCYXNlSW5mbxgCIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5m" + "bxItCg5MZWdpb25GbGFnSW5mbxgDIAEoCzIVLnR5cGVzLkxlZ2lvbkZsYWdJ" + "bmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeLegionFlagInfoReflection.Descriptor,
			TypeRankDataReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarPlayerRankDetail), (MessageParser)(object)LegionWarPlayerRankDetail.Parser, new string[3] { "RankData", "RoleBaseInfo", "LegionFlagInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
