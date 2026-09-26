using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionBossBuyCombatAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionBossBuyCombatAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkxlZ2lvbkJvc3NCdXlDb21iYXRBY2sucHJvdG8SBXR5cGVzGhV0" + "eXBlLkRhdGFDaGFuZ2UucHJvdG8aGXR5cGUuTGVnaW9uU2VsZkluZm8ucHJv" + "dG8ibgoWTGVnaW9uQm9zc0J1eUNvbWJhdEFjaxItCg5MZWdpb25TZWxmSW5m" + "bxgBIAEoCzIVLnR5cGVzLkxlZ2lvblNlbGZJbmZvEiUKCkRhdGFDaGFuZ2UY" + "AiABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionBossBuyCombatAck), (MessageParser)(object)LegionBossBuyCombatAck.Parser, new string[2] { "LegionSelfInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
