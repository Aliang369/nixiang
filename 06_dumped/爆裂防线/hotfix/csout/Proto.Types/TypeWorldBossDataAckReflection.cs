using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldBossDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossDataAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLldvcmxkQm9zc0RhdGFBY2sucHJvdG8SBXR5cGVzGhN0eXBlLlJh" + "bmtEYXRhLnByb3RvGhh0eXBlLldvcmxkQm9zc0luZm8ucHJvdG8aHHR5cGUu" + "V29ybGRCb3NzU2VsZkluZm8ucHJvdG8ioAEKEFdvcmxkQm9zc0RhdGFBY2sS" + "MwoRV29ybGRCb3NzU2VsZkluZm8YASABKAsyGC50eXBlcy5Xb3JsZEJvc3NT" + "ZWxmSW5mbxIqChFXb3JsZEJvc3NTZWxmUmFuaxgCIAEoCzIPLnR5cGVzLlJh" + "bmtEYXRhEisKDVdvcmxkQm9zc0luZm8YAyABKAsyFC50eXBlcy5Xb3JsZEJv" + "c3NJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeRankDataReflection.Descriptor,
			TypeWorldBossInfoReflection.Descriptor,
			TypeWorldBossSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossDataAck), (MessageParser)(object)WorldBossDataAck.Parser, new string[3] { "WorldBossSelfInfo", "WorldBossSelfRank", "WorldBossInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
