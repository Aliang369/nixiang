using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOpsRankDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOpsRankDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLk9wc1JhbmtEZXRhaWwucHJvdG8SBXR5cGVzGhV0eXBlLlJhbmtE" + "ZXRhaWwucHJvdG8iVQoNT3BzUmFua0RldGFpbBIMCgRSYW5rGAEgASgFEhEK" + "CVJhbmtMaW1pdBgCIAEoBRIjCghSYW5rSW5mbxgDIAEoCzIRLnR5cGVzLlJh" + "bmtEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRankDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OpsRankDetail), (MessageParser)(object)OpsRankDetail.Parser, new string[3] { "Rank", "RankLimit", "RankInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
