using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityRoomPreviewAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityRoomPreviewAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkZhY2lsaXR5Um9vbVByZXZpZXdBY2sucHJvdG8SBXR5cGVzGh10" + "eXBlLkZhY2lsaXR5Um9vbURldGFpbC5wcm90bxofdHlwZS5GYWNpbGl0eVNo" + "b3BMZWZ0SW5mby5wcm90bxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8ipgEK" + "FkZhY2lsaXR5Um9vbVByZXZpZXdBY2sSLQoKUm9vbURldGFpbBgBIAEoCzIZ" + "LnR5cGVzLkZhY2lsaXR5Um9vbURldGFpbBIqCg1Sb2JvdEJhc2VJbmZvGAIg" + "ASgLMhMudHlwZXMuUm9sZUJhc2VJbmZvEjEKDFNob3BMZWZ0SW5mbxgDIAEo" + "CzIbLnR5cGVzLkZhY2lsaXR5U2hvcExlZnRJbmZvQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeFacilityRoomDetailReflection.Descriptor,
			TypeFacilityShopLeftInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityRoomPreviewAck), (MessageParser)(object)FacilityRoomPreviewAck.Parser, new string[3] { "RoomDetail", "RobotBaseInfo", "ShopLeftInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
