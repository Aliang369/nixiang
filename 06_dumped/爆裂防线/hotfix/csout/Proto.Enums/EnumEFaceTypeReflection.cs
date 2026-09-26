using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEFaceTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEFaceTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVGYWNlVHlwZS5wcm90bxIJRUZhY2VUeXBlKlYKCUVGYWNlVHlw" + "ZRIOCgpFbXB0eVZhbHVlEAASDQoJSGFpcnN0eWxlEAESCwoHRXllYnJvdxAC" + "EgcKA0V5ZRADEgkKBUJlYXJkEAQSCQoFTW91dGgQBUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EFaceType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
