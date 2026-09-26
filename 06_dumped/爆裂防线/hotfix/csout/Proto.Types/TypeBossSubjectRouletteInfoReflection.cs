using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBossSubjectRouletteInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBossSubjectRouletteInfoReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkJvc3NTdWJqZWN0Um91bGV0dGVJbmZvLnByb3RvEgV0eXBlcxom" + "dHlwZS5MaW1pdFRpbWVSb3VsZXR0ZVJhbmREZXRhaWwucHJvdG8aHnR5cGUu" + "T3BlcmF0ZVRhcmdldERldGFpbC5wcm90byLCAQoXQm9zc1N1YmplY3RSb3Vs" + "ZXR0ZUluZm8SDQoFQ291bnQYASABKAUSNAoIU2xvdExpc3QYAiADKAsyIi50" + "eXBlcy5MaW1pdFRpbWVSb3VsZXR0ZVJhbmREZXRhaWwSLgoKVGFyZ2V0TGlz" + "dBgDIAMoCzIaLnR5cGVzLk9wZXJhdGVUYXJnZXREZXRhaWwSEQoJT25jZUx1" + "Y2t5GAQgASgFEhAKCE1heEx1Y2t5GAUgASgFEg0KBUx1Y2t5GAYgASgFQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLimitTimeRouletteRandDetailReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BossSubjectRouletteInfo), (MessageParser)(object)BossSubjectRouletteInfo.Parser, new string[6] { "Count", "SlotList", "TargetList", "OnceLucky", "MaxLucky", "Lucky" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
