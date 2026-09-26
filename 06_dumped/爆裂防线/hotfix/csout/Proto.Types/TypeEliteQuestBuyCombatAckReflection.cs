using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEliteQuestBuyCombatAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEliteQuestBuyCombatAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkVsaXRlUXVlc3RCdXlDb21iYXRBY2sucHJvdG8SBXR5cGVzGhV0" + "eXBlLkRhdGFDaGFuZ2UucHJvdG8aGXR5cGUuRWxpdGVRdWVzdEluZm8ucHJv" + "dG8ibgoWRWxpdGVRdWVzdEJ1eUNvbWJhdEFjaxItCg5FbGl0ZVF1ZXN0SW5m" + "bxgBIAEoCzIVLnR5cGVzLkVsaXRlUXVlc3RJbmZvEiUKCkRhdGFDaGFuZ2UY" + "AiABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeEliteQuestInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EliteQuestBuyCombatAck), (MessageParser)(object)EliteQuestBuyCombatAck.Parser, new string[2] { "EliteQuestInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
