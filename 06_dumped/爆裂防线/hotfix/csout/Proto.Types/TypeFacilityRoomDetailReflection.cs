using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeFacilityRoomDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityRoomDetailReflection()
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkZhY2lsaXR5Um9vbURldGFpbC5wcm90bxIFdHlwZXMaHWVudW0u" + "RUZhY2lsaXR5Um9vbVN0YXRlLnByb3RvGhxlbnVtLkVGYWNpbGl0eVJvb21U" + "eXBlLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhd0eXBlLlJvbGVC" + "YXNlSW5mby5wcm90byK9AgoSRmFjaWxpdHlSb29tRGV0YWlsEg4KBlJvb21J" + "ZBgBIAEoBRI2CghSb29tVHlwZRgCIAEoDjIkLkVGYWNpbGl0eVJvb21UeXBl" + "LkVGYWNpbGl0eVJvb21UeXBlEhQKDEV4cGxvcmVyUm9sZRgDIAEoBRIWCg5F" + "eHBsb3JlckV4cGlyZRgEIAEoAxImCglSb29tUm9ib3QYBSABKAsyEy50eXBl" + "cy5Sb2xlQmFzZUluZm8SJwoKUm9vbU9ubGluZRgGIAEoCzITLnR5cGVzLlJv" + "bGVCYXNlSW5mbxIlCglSb29tQXdhcmQYByABKAsyEi50eXBlcy5Bd2FyZERl" + "dGFpbBI5CglSb29tU3RhdGUYCCABKA4yJi5FRmFjaWxpdHlSb29tU3RhdGUu" + "RUZhY2lsaXR5Um9vbVN0YXRlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumEFacilityRoomStateReflection.Descriptor,
			EnumEFacilityRoomTypeReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityRoomDetail), (MessageParser)(object)FacilityRoomDetail.Parser, new string[8] { "RoomId", "RoomType", "ExplorerRole", "ExplorerExpire", "RoomRobot", "RoomOnline", "RoomAward", "RoomState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
