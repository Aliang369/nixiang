using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldResourceActivateAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldResourceActivateAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLldvcmxkUmVzb3VyY2VBY3RpdmF0ZUFjay5wcm90bxIFdHlwZXMa" + "FHR5cGUuV29ybGRJbmZvLnByb3RvIj8KGFdvcmxkUmVzb3VyY2VBY3RpdmF0" + "ZUFjaxIjCglXb3JsZEluZm8YASABKAsyEC50eXBlcy5Xb3JsZEluZm9CLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWorldInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldResourceActivateAck), (MessageParser)(object)WorldResourceActivateAck.Parser, new string[1] { "WorldInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
