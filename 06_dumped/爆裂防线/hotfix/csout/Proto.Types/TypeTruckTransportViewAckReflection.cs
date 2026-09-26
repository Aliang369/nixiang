using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTruckTransportViewAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckTransportViewAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlRydWNrVHJhbnNwb3J0Vmlld0Fjay5wcm90bxIFdHlwZXMaHXR5" + "cGUuVHJ1Y2tUcmFuc3BvcnRJbmZvLnByb3RvIk4KFVRydWNrVHJhbnNwb3J0" + "Vmlld0FjaxI1ChJUcnVja1RyYW5zcG9ydEluZm8YASABKAsyGS50eXBlcy5U" + "cnVja1RyYW5zcG9ydEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeTruckTransportInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckTransportViewAck), (MessageParser)(object)TruckTransportViewAck.Parser, new string[1] { "TruckTransportInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
