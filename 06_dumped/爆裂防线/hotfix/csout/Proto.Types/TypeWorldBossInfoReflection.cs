using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWorldBossInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLldvcmxkQm9zc0luZm8ucHJvdG8SBXR5cGVzGhVlbnVtLkVCb29s" + "U3RhdGUucHJvdG8ikQEKDVdvcmxkQm9zc0luZm8SDAoEVVVJRBgBIAEoBRIO" + "CgZCb3NzSWQYAiABKAUSEQoJVG90YWxIdXJ0GAMgASgDEhAKCFBhc3NIdXJ0" + "GAQgASgDEhIKClRhcmdldExpc3QYBSADKAMSKQoJUGFzc1N0YXRlGAYgASgO" + "MhYuRUJvb2xTdGF0ZS5FQm9vbFN0YXRlQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossInfo), (MessageParser)(object)WorldBossInfo.Parser, new string[6] { "UUID", "BossId", "TotalHurt", "PassHurt", "TargetList", "PassState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
