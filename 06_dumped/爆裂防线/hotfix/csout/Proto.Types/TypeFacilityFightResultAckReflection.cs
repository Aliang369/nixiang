using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityFightResultAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkZhY2lsaXR5RmlnaHRSZXN1bHRBY2sucHJvdG8SBXR5cGVzGhZ0" + "eXBlLkF3YXJkRGV0YWlsLnByb3RvGht0eXBlLkJhdHRsZVJlc3VsdEluZm8u" + "cHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoadHlwZS5GYWNpbGl0eU1h" + "cEluZm8ucHJvdG8ixgEKFkZhY2lsaXR5RmlnaHRSZXN1bHRBY2sSLQoMQmF0" + "dGxlU2V0dGxlGAEgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mbxIvCg9G" + "YWNpbGl0eU1hcEluZm8YAiABKAsyFi50eXBlcy5GYWNpbGl0eU1hcEluZm8S" + "JQoJQXdhcmRMaXN0GAMgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0" + "YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFacilityMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityFightResultAck), (MessageParser)(object)FacilityFightResultAck.Parser, new string[4] { "BattleSettle", "FacilityMapInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
