using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityEventShopAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityEventShopAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkZhY2lsaXR5RXZlbnRTaG9wQWNrLnByb3RvEgV0eXBlcxoWdHlw" + "ZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhp0" + "eXBlLkZhY2lsaXR5TWFwSW5mby5wcm90bxoddHlwZS5GYWNpbGl0eVJvb21E" + "ZXRhaWwucHJvdG8aG3R5cGUuRmFjaWxpdHlTaG9wSW5mby5wcm90byL3AQoU" + "RmFjaWxpdHlFdmVudFNob3BBY2sSLwoPRmFjaWxpdHlNYXBJbmZvGAEgASgL" + "MhYudHlwZXMuRmFjaWxpdHlNYXBJbmZvEi0KClJvb21EZXRhaWwYAiABKAsy" + "GS50eXBlcy5GYWNpbGl0eVJvb21EZXRhaWwSMQoQRmFjaWxpdHlTaG9wSW5m" + "bxgDIAEoCzIXLnR5cGVzLkZhY2lsaXR5U2hvcEluZm8SJQoJQXdhcmRMaXN0" + "GAQgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgFIAEo" + "CzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFacilityMapInfoReflection.Descriptor,
			TypeFacilityRoomDetailReflection.Descriptor,
			TypeFacilityShopInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityEventShopAck), (MessageParser)(object)FacilityEventShopAck.Parser, new string[5] { "FacilityMapInfo", "RoomDetail", "FacilityShopInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
