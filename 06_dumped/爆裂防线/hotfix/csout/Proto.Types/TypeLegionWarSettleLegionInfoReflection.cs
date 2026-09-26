using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarSettleLegionInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarSettleLegionInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkxlZ2lvbldhclNldHRsZUxlZ2lvbkluZm8ucHJvdG8SBXR5cGVz" + "GhplbnVtLkVMZWdpb25XYXJGb3JjZS5wcm90bxokdHlwZS5MZWdpb25XYXJM" + "ZWdpb25SYW5rRGV0YWlsLnByb3RvIpYBChlMZWdpb25XYXJTZXR0bGVMZWdp" + "b25JbmZvEjEKB0ZvcmNlSWQYASABKA4yIC5FTGVnaW9uV2FyRm9yY2UuRUxl" + "Z2lvbldhckZvcmNlEhIKClNjb3JlQ291bnQYAiABKAUSMgoIUmFua0xpc3QY" + "AyADKAsyIC50eXBlcy5MZWdpb25XYXJMZWdpb25SYW5rRGV0YWlsQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumELegionWarForceReflection.Descriptor,
			TypeLegionWarLegionRankDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarSettleLegionInfo), (MessageParser)(object)LegionWarSettleLegionInfo.Parser, new string[3] { "ForceId", "ScoreCount", "RankList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
