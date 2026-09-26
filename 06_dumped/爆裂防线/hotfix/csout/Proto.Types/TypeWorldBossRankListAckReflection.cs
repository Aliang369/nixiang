using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldBossRankListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossRankListAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLldvcmxkQm9zc1JhbmtMaXN0QWNrLnByb3RvEgV0eXBlcxoVdHlw" + "ZS5SYW5rRGV0YWlsLnByb3RvIkkKFFdvcmxkQm9zc1JhbmtMaXN0QWNrEgwK" + "BFVVSUQYASABKAUSIwoIUmFua0xpc3QYAiADKAsyES50eXBlcy5SYW5rRGV0" + "YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRankDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossRankListAck), (MessageParser)(object)WorldBossRankListAck.Parser, new string[2] { "UUID", "RankList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
