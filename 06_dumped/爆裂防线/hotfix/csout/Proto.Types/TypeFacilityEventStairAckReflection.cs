using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityEventStairAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityEventStairAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkZhY2lsaXR5RXZlbnRTdGFpckFjay5wcm90bxIFdHlwZXMaFnR5" + "cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoa" + "dHlwZS5GYWNpbGl0eU1hcEluZm8ucHJvdG8aHXR5cGUuRmFjaWxpdHlSb29t" + "RGV0YWlsLnByb3RvIsUBChVGYWNpbGl0eUV2ZW50U3RhaXJBY2sSLwoPRmFj" + "aWxpdHlNYXBJbmZvGAEgASgLMhYudHlwZXMuRmFjaWxpdHlNYXBJbmZvEi0K" + "ClJvb21EZXRhaWwYAiABKAsyGS50eXBlcy5GYWNpbGl0eVJvb21EZXRhaWwS" + "JQoJQXdhcmRMaXN0GAMgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0" + "YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFacilityMapInfoReflection.Descriptor,
			TypeFacilityRoomDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityEventStairAck), (MessageParser)(object)FacilityEventStairAck.Parser, new string[4] { "FacilityMapInfo", "RoomDetail", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
