using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEOperatePlanTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEOperatePlanTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVPcGVyYXRlUGxhblR5cGUucHJvdG8SEEVPcGVyYXRlUGxhblR5" + "cGUqTQoQRU9wZXJhdGVQbGFuVHlwZRIOCgpFbXB0eVZhbHVlEAASDQoJTmV3" + "U2VydmVyEAESDQoJT2xkU2VydmVyEAISCwoHSG9saWRheRADQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EOperatePlanType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
