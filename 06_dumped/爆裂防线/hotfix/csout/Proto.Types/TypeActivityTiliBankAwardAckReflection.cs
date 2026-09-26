using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeActivityTiliBankAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeActivityTiliBankAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkFjdGl2aXR5VGlsaUJhbmtBd2FyZEFjay5wcm90bxIFdHlwZXMa" + "F3R5cGUuQWN0aXZpdHlJbmZvLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnBy" + "b3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8ikwEKGEFjdGl2aXR5VGlsaUJh" + "bmtBd2FyZEFjaxIpCgxBY3Rpdml0eUluZm8YASABKAsyEy50eXBlcy5BY3Rp" + "dml0eUluZm8SJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdhcmREZXRh" + "aWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeActivityInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ActivityTiliBankAwardAck), (MessageParser)(object)ActivityTiliBankAwardAck.Parser, new string[3] { "ActivityInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
