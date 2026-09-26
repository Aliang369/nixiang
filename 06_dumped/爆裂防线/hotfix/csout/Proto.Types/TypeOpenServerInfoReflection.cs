using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOpenServerInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOpenServerInfoReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLk9wZW5TZXJ2ZXJJbmZvLnByb3RvEgV0eXBlcxocdHlwZS5PcHNS" + "YW5rQmFzZURldGFpbC5wcm90byJTCg5PcGVuU2VydmVySW5mbxIuCgxSYW5r" + "QmFzZUxpc3QYASADKAsyGC50eXBlcy5PcHNSYW5rQmFzZURldGFpbBIRCglS" + "YW5rRW5kVG0YAiABKANCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeOpsRankBaseDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OpenServerInfo), (MessageParser)(object)OpenServerInfo.Parser, new string[2] { "RankBaseList", "RankEndTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
