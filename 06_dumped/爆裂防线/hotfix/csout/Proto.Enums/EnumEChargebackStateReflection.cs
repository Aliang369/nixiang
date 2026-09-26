using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEChargebackStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEChargebackStateReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVDaGFyZ2ViYWNrU3RhdGUucHJvdG8SEEVDaGFyZ2ViYWNrU3Rh" + "dGUqNQoQRUNoYXJnZWJhY2tTdGF0ZRIICgROb25lEAASCwoHV2FybmluZxAB" + "EgoKBkRhbmdlchACQi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJn" + "YW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EChargebackState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
