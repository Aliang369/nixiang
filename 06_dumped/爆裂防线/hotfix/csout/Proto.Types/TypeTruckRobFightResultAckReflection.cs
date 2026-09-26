using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeTruckRobFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckRobFightResultAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlRydWNrUm9iRmlnaHRSZXN1bHRBY2sucHJvdG8SBXR5cGVzGhVl" + "bnVtLkVCb29sU3RhdGUucHJvdG8aFnR5cGUuQXdhcmREZXRhaWwucHJvdG8a" + "G3R5cGUuQmF0dGxlUmVzdWx0SW5mby5wcm90bxoVdHlwZS5EYXRhQ2hhbmdl" + "LnByb3RvGhh0eXBlLlRydWNrU2VsZkluZm8ucHJvdG8i7QEKFlRydWNrUm9i" + "RmlnaHRSZXN1bHRBY2sSKwoNVHJ1Y2tTZWxmSW5mbxgBIAEoCzIULnR5cGVz" + "LlRydWNrU2VsZkluZm8SLQoMQmF0dGxlU2V0dGxlGAIgASgLMhcudHlwZXMu" + "QmF0dGxlUmVzdWx0SW5mbxIlCglBd2FyZExpc3QYAyADKAsyEi50eXBlcy5B" + "d2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAQgASgLMhEudHlwZXMuRGF0YUNo" + "YW5nZRIpCglCZXN0UHJpemUYBSABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3Rh" + "dGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeTruckSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckRobFightResultAck), (MessageParser)(object)TruckRobFightResultAck.Parser, new string[5] { "TruckSelfInfo", "BattleSettle", "AwardList", "DataChange", "BestPrize" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
