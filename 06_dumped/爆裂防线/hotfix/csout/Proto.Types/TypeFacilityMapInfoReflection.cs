using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityMapInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityMapInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkZhY2lsaXR5TWFwSW5mby5wcm90bxIFdHlwZXMaG3R5cGUuRmFj" + "aWxpdHlCYXNlSW5mby5wcm90bxoddHlwZS5GYWNpbGl0eVJvb21EZXRhaWwu" + "cHJvdG8ilAEKD0ZhY2lsaXR5TWFwSW5mbxIpCghCYXNlSW5mbxgBIAEoCzIX" + "LnR5cGVzLkZhY2lsaXR5QmFzZUluZm8SKwoIUm9vbUxpc3QYAiADKAsyGS50" + "eXBlcy5GYWNpbGl0eVJvb21EZXRhaWwSEwoLQ3VycmVudFJvb20YAyABKAUS" + "FAoMRXhwbG9yZUNvdW50GAQgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeFacilityBaseInfoReflection.Descriptor,
			TypeFacilityRoomDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityMapInfo), (MessageParser)(object)FacilityMapInfo.Parser, new string[4] { "BaseInfo", "RoomList", "CurrentRoom", "ExploreCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
