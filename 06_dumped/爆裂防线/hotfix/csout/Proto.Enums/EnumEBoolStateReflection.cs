using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEBoolStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEBoolStateReflection()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVlbnVtLkVCb29sU3RhdGUucHJvdG8SCkVCb29sU3RhdGUqIQoKRUJvb2xT" + "dGF0ZRIJCgVGYWxzZRAAEggKBFRydWUQAUIuWgkuLztwcm90b2OqAgtQcm90" + "by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EBoolState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
