using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityStairTeleportAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityStairTeleportAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkZhY2lsaXR5U3RhaXJUZWxlcG9ydEFjay5wcm90bxIFdHlwZXMa" + "GnR5cGUuRmFjaWxpdHlNYXBJbmZvLnByb3RvIksKGEZhY2lsaXR5U3RhaXJU" + "ZWxlcG9ydEFjaxIvCg9GYWNpbGl0eU1hcEluZm8YASABKAsyFi50eXBlcy5G" + "YWNpbGl0eU1hcEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeFacilityMapInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityStairTeleportAck), (MessageParser)(object)FacilityStairTeleportAck.Parser, new string[1] { "FacilityMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
