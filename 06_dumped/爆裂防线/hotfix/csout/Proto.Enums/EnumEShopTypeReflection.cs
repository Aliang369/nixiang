using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEShopTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEShopTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVTaG9wVHlwZS5wcm90bxIJRVNob3BUeXBlKlIKCUVTaG9wVHlw" + "ZRIOCgpFbXB0eVZhbHVlEAASCQoFU2hvcDEQARIJCgVTaG9wMhACEgkKBVNo" + "b3AzEAMSCQoFU2hvcDQQBBIJCgVTaG9wNRAFQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EShopType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
