using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleCreateReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleCreateReqReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlJvbGVDcmVhdGVSZXEucHJvdG8SBXR5cGVzIkMKDVJvbGVDcmVh" + "dGVSZXESEAoIQXZhdGFySWQYASABKAUSDgoGSGVhZElkGAIgASgFEhAKCFJv" + "bGVOYW1lGAMgASgJQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleCreateReq), (MessageParser)(object)RoleCreateReq.Parser, new string[3] { "AvatarId", "HeadId", "RoleName" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
