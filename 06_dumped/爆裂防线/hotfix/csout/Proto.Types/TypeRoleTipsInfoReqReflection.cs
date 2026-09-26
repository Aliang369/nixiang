using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleTipsInfoReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleTipsInfoReqReflection()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLlJvbGVUaXBzSW5mb1JlcS5wcm90bxIFdHlwZXMiIwoPUm9sZVRp" + "cHNJbmZvUmVxEhAKCFBsYXllcklkGAEgASgFQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleTipsInfoReq), (MessageParser)(object)RoleTipsInfoReq.Parser, new string[1] { "PlayerId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
