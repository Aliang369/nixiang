using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEliteQuestFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEliteQuestFightStartAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkVsaXRlUXVlc3RGaWdodFN0YXJ0QWNrLnByb3RvEgV0eXBlcxoV" + "dHlwZS5CYXR0bGVJbmZvLnByb3RvImAKF0VsaXRlUXVlc3RGaWdodFN0YXJ0" + "QWNrEg8KB0xldmVsSWQYASABKAUSDQoFQ291bnQYAiABKAUSJQoKQmF0dGxl" + "SW5mbxgDIAEoCzIRLnR5cGVzLkJhdHRsZUluZm9CLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EliteQuestFightStartAck), (MessageParser)(object)EliteQuestFightStartAck.Parser, new string[3] { "LevelId", "Count", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
