using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEMineWarServerStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEMineWarServerStateReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch5lbnVtLkVNaW5lV2FyU2VydmVyU3RhdGUucHJvdG8SE0VNaW5lV2FyU2Vy" + "dmVyU3RhdGUqTQoTRU1pbmVXYXJTZXJ2ZXJTdGF0ZRIOCgpFbXB0eVZhbHVl" + "EAASCgoGQmFycmVuEAESCgoGTm9ybWFsEAISDgoKUHJvc3Blcml0eRADQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1z" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EMineWarServerState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
