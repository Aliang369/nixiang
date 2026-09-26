using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaFightResultAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkFyZW5hRmlnaHRSZXN1bHRBY2sucHJvdG8SBXR5cGVzGhR0eXBl" + "LkFyZW5hSW5mby5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxobdHlw" + "ZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJv" + "dG8itwEKE0FyZW5hRmlnaHRSZXN1bHRBY2sSLQoMQmF0dGxlU2V0dGxlGAEg" + "ASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mbxIjCglBcmVuYUluZm8YAiAB" + "KAsyEC50eXBlcy5BcmVuYUluZm8SJQoJQXdhcmRMaXN0GAMgAygLMhIudHlw" + "ZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRh" + "dGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeArenaInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaFightResultAck), (MessageParser)(object)ArenaFightResultAck.Parser, new string[4] { "BattleSettle", "ArenaInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
