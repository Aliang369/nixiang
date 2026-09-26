using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOpsRankInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOpsRankInfoReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLk9wc1JhbmtJbmZvLnByb3RvEgV0eXBlcxoUZW51bS5FUmFua1R5" + "cGUucHJvdG8aGHR5cGUuT3BzUmFua0RldGFpbC5wcm90byKCAQoLT3BzUmFu" + "a0luZm8SJgoIUmFua1R5cGUYASABKA4yFC5FUmFua1R5cGUuRVJhbmtUeXBl" + "EiYKCFJhbmtMaXN0GAIgAygLMhQudHlwZXMuT3BzUmFua0RldGFpbBIRCglS" + "ZWZyZXNoVG0YAyABKAMSEAoIU2V0dGxlVG0YBCABKANCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumERankTypeReflection.Descriptor,
			TypeOpsRankDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OpsRankInfo), (MessageParser)(object)OpsRankInfo.Parser, new string[4] { "RankType", "RankList", "RefreshTm", "SettleTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
