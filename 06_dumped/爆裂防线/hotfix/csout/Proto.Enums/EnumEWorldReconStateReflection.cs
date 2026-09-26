using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEWorldReconStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEWorldReconStateReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVXb3JsZFJlY29uU3RhdGUucHJvdG8SEEVXb3JsZFJlY29uU3Rh" + "dGUqRwoQRVdvcmxkUmVjb25TdGF0ZRIOCgpFbXB0eVZhbHVlEAASCAoESWRs" + "ZRABEgsKB0V4cGxvcmUQAhIMCghFeHBsb3JlZBADQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EWorldReconState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
