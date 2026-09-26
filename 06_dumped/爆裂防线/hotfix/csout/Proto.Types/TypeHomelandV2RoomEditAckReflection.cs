using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2RoomEditAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2RoomEditAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkhvbWVsYW5kVjJSb29tRWRpdEFjay5wcm90bxIFdHlwZXMaF3R5" + "cGUuSG9tZWxhbmRJbmZvLnByb3RvIkIKFUhvbWVsYW5kVjJSb29tRWRpdEFj" + "axIpCgxIb21lbGFuZEluZm8YASABKAsyEy50eXBlcy5Ib21lbGFuZEluZm9C" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeHomelandInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2RoomEditAck), (MessageParser)(object)HomelandV2RoomEditAck.Parser, new string[1] { "HomelandInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
