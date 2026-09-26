using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEGoodsTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEGoodsTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVlbnVtLkVHb29kc1R5cGUucHJvdG8SCkVHb29kc1R5cGUqaAoKRUdvb2Rz" + "VHlwZRIOCgpFbXB0eVZhbHVlEAASCQoFUmVmZXIQARIMCghSZWNoYXJnZRAC" + "EggKBEZ1bmMQAxILCgdPcGVyYXRlEAQSCAoER2lmdBAFEhAKDE5vbk9wZXJh" + "dGluZxAGQi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFBy" + "b3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EGoodsType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
