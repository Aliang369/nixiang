using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonStageTaskInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonStageTaskInfoAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlNlYXNvblN0YWdlVGFza0luZm9BY2sucHJvdG8SBXR5cGVzGh50" + "eXBlLlNlYXNvblN0YWdlVGFza0l0ZW0ucHJvdG8iUgoWU2Vhc29uU3RhZ2VU" + "YXNrSW5mb0FjaxINCgVQaGFzZRgBIAEoBRIpCgVUYXNrcxgCIAMoCzIaLnR5" + "cGVzLlNlYXNvblN0YWdlVGFza0l0ZW1CLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonStageTaskItemReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonStageTaskInfoAck), (MessageParser)(object)SeasonStageTaskInfoAck.Parser, new string[2] { "Phase", "Tasks" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
