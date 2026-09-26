using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityEventLockedChestAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityEventLockedChestAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLkZhY2lsaXR5RXZlbnRMb2NrZWRDaGVzdEFjay5wcm90bxIFdHlw" + "ZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5w" + "cm90bxoadHlwZS5GYWNpbGl0eU1hcEluZm8ucHJvdG8aHXR5cGUuRmFjaWxp" + "dHlSb29tRGV0YWlsLnByb3RvIssBChtGYWNpbGl0eUV2ZW50TG9ja2VkQ2hl" + "c3RBY2sSLwoPRmFjaWxpdHlNYXBJbmZvGAEgASgLMhYudHlwZXMuRmFjaWxp" + "dHlNYXBJbmZvEi0KClJvb21EZXRhaWwYAiABKAsyGS50eXBlcy5GYWNpbGl0" + "eVJvb21EZXRhaWwSJQoJQXdhcmRMaXN0GAMgAygLMhIudHlwZXMuQXdhcmRE" + "ZXRhaWwSJQoKRGF0YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFacilityMapInfoReflection.Descriptor,
			TypeFacilityRoomDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityEventLockedChestAck), (MessageParser)(object)FacilityEventLockedChestAck.Parser, new string[4] { "FacilityMapInfo", "RoomDetail", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
