using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonEventTriggerReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonEventTriggerReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLldvcmxkRHVuZ2VvbkV2ZW50VHJpZ2dlclJlcS5wcm90bxIFdHlw" + "ZXMiPQobV29ybGREdW5nZW9uRXZlbnRUcmlnZ2VyUmVxEg4KBkNlbGxJZBgB" + "IAEoCRIOCgZHcmlkSWQYAiABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonEventTriggerReq), (MessageParser)(object)WorldDungeonEventTriggerReq.Parser, new string[2] { "CellId", "GridId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
