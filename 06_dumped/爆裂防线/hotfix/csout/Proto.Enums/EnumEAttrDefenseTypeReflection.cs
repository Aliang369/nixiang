using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEAttrDefenseTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEAttrDefenseTypeReflection()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVBdHRyRGVmZW5zZVR5cGUucHJvdG8SEEVBdHRyRGVmZW5zZVR5" + "cGUqpgEKEEVBdHRyRGVmZW5zZVR5cGUSDgoKRW1wdHlWYWx1ZRAAEg0KCURl" + "ZmVuc2VIcBALEgwKCFNoaWVsZEhwEAwSDgoKRGFtYWdlRGVjchANEhIKDkRh" + "bWFnZUltbXVuaXR5EA4SDwoLRW5lcmd5UmVnZW4QDxIPCgtGYWlsdXJlUmF0" + "ZRAVEg4KCk1heE1pbGVhZ2UQFhIPCgtWaXNpb25SYW5nZRAXQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EAttrDefenseType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
