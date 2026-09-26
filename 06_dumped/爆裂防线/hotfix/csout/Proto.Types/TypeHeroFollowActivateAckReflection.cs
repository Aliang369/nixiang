using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHeroFollowActivateAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroFollowActivateAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkhlcm9Gb2xsb3dBY3RpdmF0ZUFjay5wcm90bxIFdHlwZXMaGXR5" + "cGUuSGVyb0ZvbGxvd0luZm8ucHJvdG8iRgoVSGVyb0ZvbGxvd0FjdGl2YXRl" + "QWNrEi0KDkhlcm9Gb2xsb3dJbmZvGAEgASgLMhUudHlwZXMuSGVyb0ZvbGxv" + "d0luZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeHeroFollowInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroFollowActivateAck), (MessageParser)(object)HeroFollowActivateAck.Parser, new string[1] { "HeroFollowInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
