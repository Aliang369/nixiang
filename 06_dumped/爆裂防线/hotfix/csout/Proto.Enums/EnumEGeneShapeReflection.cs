using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEGeneShapeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEGeneShapeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVlbnVtLkVHZW5lU2hhcGUucHJvdG8SCkVHZW5lU2hhcGUqXAoKRUdlbmVT" + "aGFwZRIOCgpFbXB0eVZhbHVlEAASCgoGQ2lyY2xlEAESCgoGU3F1YXJlEAIS" + "DAoIVHJpYW5nbGUQAxILCgdSaG9tYnVzEAQSCwoHSGV4YWdvbhAFQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EGeneShape) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
