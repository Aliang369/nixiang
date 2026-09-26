using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateLimitTimeVendorTargetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateLimitTimeVendorTargetAckReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLk9wZXJhdGVMaW1pdFRpbWVWZW5kb3JUYXJnZXRBY2sucHJvdG8S" + "BXR5cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFu" + "Z2UucHJvdG8aInR5cGUuTGltaXRUaW1lUm91bGV0dGVEZXRhaWwucHJvdG8i" + "rAEKH09wZXJhdGVMaW1pdFRpbWVWZW5kb3JUYXJnZXRBY2sSOwoTTGltaXRU" + "aW1lVmVuZG9yTGlzdBgBIAMoCzIeLnR5cGVzLkxpbWl0VGltZVJvdWxldHRl" + "RGV0YWlsEiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWls" + "EiUKCkRhdGFDaGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLimitTimeRouletteDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateLimitTimeVendorTargetAck), (MessageParser)(object)OperateLimitTimeVendorTargetAck.Parser, new string[3] { "LimitTimeVendorList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
