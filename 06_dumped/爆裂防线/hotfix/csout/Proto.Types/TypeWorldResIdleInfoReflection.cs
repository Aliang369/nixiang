using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldResIdleInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldResIdleInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLldvcmxkUmVzSWRsZUluZm8ucHJvdG8SBXR5cGVzGhZ0eXBlLkF3" + "YXJkRGV0YWlsLnByb3RvInUKEFdvcmxkUmVzSWRsZUluZm8SEQoJU2V0dGxl" + "TWF4GAEgASgFEhQKDFNldHRsZVNlY29uZBgCIAEoBRIOCgZHYWluVG0YAyAB" + "KAMSKAoMQmFnQXdhcmRMaXN0GAQgAygLMhIudHlwZXMuQXdhcmREZXRhaWxC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldResIdleInfo), (MessageParser)(object)WorldResIdleInfo.Parser, new string[4] { "SettleMax", "SettleSecond", "GainTm", "BagAwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
