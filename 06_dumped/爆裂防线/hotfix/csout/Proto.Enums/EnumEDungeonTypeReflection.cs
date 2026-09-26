using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEDungeonTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEDungeonTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVEdW5nZW9uVHlwZS5wcm90bxIMRUR1bmdlb25UeXBlKjgKDEVE" + "dW5nZW9uVHlwZRIOCgpFbXB0eVZhbHVlEAASDQoJSG9tZUZsb29yEAESCQoF" + "V29ybGQQAkIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQ" + "cm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EDungeonType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
