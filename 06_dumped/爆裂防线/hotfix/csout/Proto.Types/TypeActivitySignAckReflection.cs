using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeActivitySignAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeActivitySignAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkFjdGl2aXR5U2lnbkFjay5wcm90bxIFdHlwZXMaF3R5cGUuQWN0" + "aXZpdHlJbmZvLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBl" + "LkRhdGFDaGFuZ2UucHJvdG8iigEKD0FjdGl2aXR5U2lnbkFjaxIpCgxBY3Rp" + "dml0eUluZm8YASABKAsyEy50eXBlcy5BY3Rpdml0eUluZm8SJQoJQXdhcmRM" + "aXN0GAIgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgD" + "IAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeActivityInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ActivitySignAck), (MessageParser)(object)ActivitySignAck.Parser, new string[3] { "ActivityInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
