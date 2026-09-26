using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEUnlockTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEUnlockTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZlbnVtLkVVbmxvY2tUeXBlLnByb3RvEgtFVW5sb2NrVHlwZSo5CgtFVW5s" + "b2NrVHlwZRIOCgpFbXB0eVZhbHVlEAASDQoJUm9sZUxldmVsEAESCwoHUGVl" + "cmFnZRACQi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFBy" + "b3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EUnlockType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
