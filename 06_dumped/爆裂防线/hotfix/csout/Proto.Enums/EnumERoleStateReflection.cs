using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumERoleStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumERoleStateReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVlbnVtLkVSb2xlU3RhdGUucHJvdG8SCkVSb2xlU3RhdGUqMAoKRVJvbGVT" + "dGF0ZRIICgRJbml0EAASDAoIQWN0aXZhdGUQARIKCgZPbmxpbmUQAkIuWgku" + "Lztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ERoleState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
