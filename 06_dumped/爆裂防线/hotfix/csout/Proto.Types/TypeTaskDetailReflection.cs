using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeTaskDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTaskDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLlRhc2tEZXRhaWwucHJvdG8SBXR5cGVzGhdlbnVtLkVBY3RpdmVT" + "dGF0ZS5wcm90byKHAQoKVGFza0RldGFpbBIQCghUYXNrVHlwZRgBIAEoBRIO" + "CgZUYXNrSWQYAiABKAUSDQoFUGFyYW0YAyABKAUSDgoGVGFyZ2V0GAQgASgF" + "Eg0KBVJlYWNoGAUgASgFEikKBVN0YXRlGAYgASgOMhouRUFjdGl2ZVN0YXRl" + "LkVBY3RpdmVTdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEActiveStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TaskDetail), (MessageParser)(object)TaskDetail.Parser, new string[6] { "TaskType", "TaskId", "Param", "Target", "Reach", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
