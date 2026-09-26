using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeActivityTiliBankAwardAllAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeActivityTiliBankAwardAllAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLkFjdGl2aXR5VGlsaUJhbmtBd2FyZEFsbEFjay5wcm90bxIFdHlw" + "ZXMaF3R5cGUuQWN0aXZpdHlJbmZvLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWls" + "LnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8ilgEKG0FjdGl2aXR5VGls" + "aUJhbmtBd2FyZEFsbEFjaxIpCgxBY3Rpdml0eUluZm8YASABKAsyEy50eXBl" + "cy5BY3Rpdml0eUluZm8SJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdh" + "cmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFu" + "Z2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeActivityInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ActivityTiliBankAwardAllAck), (MessageParser)(object)ActivityTiliBankAwardAllAck.Parser, new string[3] { "ActivityInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
