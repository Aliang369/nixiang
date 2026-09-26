using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumECommonStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumECommonStateReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVDb21tb25TdGF0ZS5wcm90bxIMRUNvbW1vblN0YXRlKigKDEVD" + "b21tb25TdGF0ZRIGCgJObxAAEgcKA1llcxABEgcKA0VuZBACQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ECommonState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
