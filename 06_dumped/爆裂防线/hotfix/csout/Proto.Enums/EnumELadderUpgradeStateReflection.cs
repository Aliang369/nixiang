using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumELadderUpgradeStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumELadderUpgradeStateReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch5lbnVtLkVMYWRkZXJVcGdyYWRlU3RhdGUucHJvdG8SE0VMYWRkZXJVcGdy" + "YWRlU3RhdGUqOAoTRUxhZGRlclVwZ3JhZGVTdGF0ZRIOCgpFbXB0eVZhbHVl" + "EAASBwoDV2luEAESCAoERmFpbBACQi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVu" + "dW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ELadderUpgradeState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
