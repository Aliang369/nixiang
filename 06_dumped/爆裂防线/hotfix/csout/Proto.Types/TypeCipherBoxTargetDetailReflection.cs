using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeCipherBoxTargetDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCipherBoxTargetDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkNpcGhlckJveFRhcmdldERldGFpbC5wcm90bxIFdHlwZXMaF2Vu" + "dW0uRUNvbW1vblN0YXRlLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3Rv" + "InUKFUNpcGhlckJveFRhcmdldERldGFpbBIOCgZDaXBoZXIYASABKAkSKQoF" + "U3RhdGUYAiABKA4yGi5FQ29tbW9uU3RhdGUuRUNvbW1vblN0YXRlEiEKBUF3" + "YXJkGAMgASgLMhIudHlwZXMuQXdhcmREZXRhaWxCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CipherBoxTargetDetail), (MessageParser)(object)CipherBoxTargetDetail.Parser, new string[3] { "Cipher", "State", "Award" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
