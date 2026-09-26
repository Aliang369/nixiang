using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSettleRankInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSettleRankInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlNldHRsZVJhbmtJbmZvLnByb3RvEgV0eXBlcyJcCg5TZXR0bGVS" + "YW5rSW5mbxIPCgdPbGRSYW5rGAEgASgFEg8KB05ld1JhbmsYAiABKAUSEwoL" + "T2xkQmVzdFJhbmsYAyABKAUSEwoLTmV3QmVzdFJhbmsYBCABKAVCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SettleRankInfo), (MessageParser)(object)SettleRankInfo.Parser, new string[4] { "OldRank", "NewRank", "OldBestRank", "NewBestRank" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
