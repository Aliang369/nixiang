using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOpsRankTopDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOpsRankTopDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLk9wc1JhbmtUb3BEZXRhaWwucHJvdG8SBXR5cGVzGhRlbnVtLkVS" + "YW5rVHlwZS5wcm90bxoVdHlwZS5SYW5rRGV0YWlsLnByb3RvInEKEE9wc1Jh" + "bmtUb3BEZXRhaWwSJgoIUmFua1R5cGUYASABKA4yFC5FUmFua1R5cGUuRVJh" + "bmtUeXBlEiMKCFJhbmtJbmZvGAIgASgLMhEudHlwZXMuUmFua0RldGFpbBIQ" + "CghTZXR0bGVUbRgDIAEoA0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumERankTypeReflection.Descriptor,
			TypeRankDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OpsRankTopDetail), (MessageParser)(object)OpsRankTopDetail.Parser, new string[3] { "RankType", "RankInfo", "SettleTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
