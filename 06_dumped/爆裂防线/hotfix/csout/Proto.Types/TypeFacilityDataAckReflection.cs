using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityDataAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkZhY2lsaXR5RGF0YUFjay5wcm90bxIFdHlwZXMaG3R5cGUuRmFj" + "aWxpdHlTZWxmSW5mby5wcm90byJECg9GYWNpbGl0eURhdGFBY2sSMQoQRmFj" + "aWxpdHlTZWxmSW5mbxgBIAEoCzIXLnR5cGVzLkZhY2lsaXR5U2VsZkluZm9C" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeFacilitySelfInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityDataAck), (MessageParser)(object)FacilityDataAck.Parser, new string[1] { "FacilitySelfInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
