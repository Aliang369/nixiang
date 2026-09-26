using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaSaoDangAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaSaoDangAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkFyZW5hU2FvRGFuZ0Fjay5wcm90bxIFdHlwZXMaFHR5cGUuQXJl" + "bmFJbmZvLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRh" + "dGFDaGFuZ2UucHJvdG8ihAEKD0FyZW5hU2FvRGFuZ0FjaxIjCglBcmVuYUlu" + "Zm8YASABKAsyEC50eXBlcy5BcmVuYUluZm8SJQoJQXdhcmRMaXN0GAIgAygL" + "MhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5" + "cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeArenaInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaSaoDangAck), (MessageParser)(object)ArenaSaoDangAck.Parser, new string[3] { "ArenaInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
