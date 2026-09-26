using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandInfoReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLkhvbWVsYW5kSW5mby5wcm90bxIFdHlwZXMaG3R5cGUuSG9tZWxh" + "bmRCYXNlSW5mby5wcm90bxoddHlwZS5Ib21lbGFuZFJvb21EZXRhaWwucHJv" + "dG8aF3R5cGUuV29ya2VyRGV0YWlsLnByb3RvIo8BCgxIb21lbGFuZEluZm8S" + "KQoIQmFzZUluZm8YASABKAsyFy50eXBlcy5Ib21lbGFuZEJhc2VJbmZvEisK" + "CFJvb21MaXN0GAIgAygLMhkudHlwZXMuSG9tZWxhbmRSb29tRGV0YWlsEicK" + "Cldvcmtlckxpc3QYAyADKAsyEy50eXBlcy5Xb3JrZXJEZXRhaWxCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeHomelandBaseInfoReflection.Descriptor,
			TypeHomelandRoomDetailReflection.Descriptor,
			TypeWorkerDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandInfo), (MessageParser)(object)HomelandInfo.Parser, new string[3] { "BaseInfo", "RoomList", "WorkerList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
