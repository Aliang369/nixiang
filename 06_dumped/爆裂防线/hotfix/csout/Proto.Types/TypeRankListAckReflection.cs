using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRankListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRankListAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLlJhbmtMaXN0QWNrLnByb3RvEgV0eXBlcxoUZW51bS5FUmFua1R5" + "cGUucHJvdG8aE3R5cGUuUmFua0RhdGEucHJvdG8aFXR5cGUuUmFua0RldGFp" + "bC5wcm90byJ9CgtSYW5rTGlzdEFjaxImCghSYW5rVHlwZRgBIAEoDjIULkVS" + "YW5rVHlwZS5FUmFua1R5cGUSIwoIUmFua0xpc3QYAiADKAsyES50eXBlcy5S" + "YW5rRGV0YWlsEiEKCFNlbGZSYW5rGAMgASgLMg8udHlwZXMuUmFua0RhdGFC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumERankTypeReflection.Descriptor,
			TypeRankDataReflection.Descriptor,
			TypeRankDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RankListAck), (MessageParser)(object)RankListAck.Parser, new string[3] { "RankType", "RankList", "SelfRank" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
