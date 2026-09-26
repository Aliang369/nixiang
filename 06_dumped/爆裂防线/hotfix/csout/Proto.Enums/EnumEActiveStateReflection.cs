using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEActiveStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEActiveStateReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVBY3RpdmVTdGF0ZS5wcm90bxIMRUFjdGl2ZVN0YXRlKjEKDEVB" + "Y3RpdmVTdGF0ZRILCgdQZW5kaW5nEAASCgoGQWN0aXZlEAESCAoERG9uZRAC" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVu" + "dW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EActiveState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
