using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityEventMonsterAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityEventMonsterAckReflection()
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkZhY2lsaXR5RXZlbnRNb25zdGVyQWNrLnByb3RvEgV0eXBlcxoW" + "dHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5CYXR0bGVJbmZvLnByb3Rv" + "GhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aGnR5cGUuRmFjaWxpdHlNYXBJbmZv" + "LnByb3RvGh10eXBlLkZhY2lsaXR5Um9vbURldGFpbC5wcm90bxoXdHlwZS5S" + "b2xlQmFzZUluZm8ucHJvdG8imgIKF0ZhY2lsaXR5RXZlbnRNb25zdGVyQWNr" + "Ei8KD0ZhY2lsaXR5TWFwSW5mbxgBIAEoCzIWLnR5cGVzLkZhY2lsaXR5TWFw" + "SW5mbxItCgpSb29tRGV0YWlsGAIgASgLMhkudHlwZXMuRmFjaWxpdHlSb29t" + "RGV0YWlsEiUKCkJhdHRsZUluZm8YAyABKAsyES50eXBlcy5CYXR0bGVJbmZv" + "EioKDVJvYm90QmFzZUluZm8YBCABKAsyEy50eXBlcy5Sb2xlQmFzZUluZm8S" + "JQoJQXdhcmRMaXN0GAUgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0" + "YUNoYW5nZRgGIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFacilityMapInfoReflection.Descriptor,
			TypeFacilityRoomDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityEventMonsterAck), (MessageParser)(object)FacilityEventMonsterAck.Parser, new string[6] { "FacilityMapInfo", "RoomDetail", "BattleInfo", "RobotBaseInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
