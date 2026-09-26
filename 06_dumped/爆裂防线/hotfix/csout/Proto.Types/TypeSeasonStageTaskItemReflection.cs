using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonStageTaskItemReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonStageTaskItemReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlNlYXNvblN0YWdlVGFza0l0ZW0ucHJvdG8SBXR5cGVzIp4BChNT" + "ZWFzb25TdGFnZVRhc2tJdGVtEgsKA1RpZBgBIAEoBRIQCghUYXNrVHlwZRgC" + "IAEoBRIOCgZTdGVwSWQYAyABKAUSEgoKVGFyZ2V0VHlwZRgEIAEoBRIQCghQ" + "cm9ncmVzcxgFIAEoBRIOCgZUYXJnZXQYBiABKAUSEQoJUGFyYW1MaXN0GAcg" + "AygFEg8KB0NsYWltZWQYCCABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonStageTaskItem), (MessageParser)(object)SeasonStageTaskItem.Parser, new string[8] { "Tid", "TaskType", "StepId", "TargetType", "Progress", "Target", "ParamList", "Claimed" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
