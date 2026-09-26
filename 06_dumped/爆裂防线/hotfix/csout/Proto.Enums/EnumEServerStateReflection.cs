using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEServerStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEServerStateReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVTZXJ2ZXJTdGF0ZS5wcm90bxIMRVNlcnZlclN0YXRlKlEKDEVT" + "ZXJ2ZXJTdGF0ZRIJCgVDbG9zZRAAEgoKBk5vcm1hbBABEgcKA0hvdBACEg0K" + "CVJlY29tbWVuZBADEhIKBUxpbWl0EP///////////wFCLloJLi87cHJvdG9j" + "qgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EServerState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
