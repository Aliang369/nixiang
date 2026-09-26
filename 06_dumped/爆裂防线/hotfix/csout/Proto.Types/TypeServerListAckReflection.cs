using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeServerListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServerListAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlNlcnZlckxpc3RBY2sucHJvdG8SBXR5cGVzGhd0eXBlLlJvbGVC" + "YXNlSW5mby5wcm90bxoXdHlwZS5TZXJ2ZXJEZXRhaWwucHJvdG8icQoNU2Vy" + "dmVyTGlzdEFjaxIQCghTZXJ2ZXJJZBgBIAEoBRInCgpTZXJ2ZXJMaXN0GAIg" + "AygLMhMudHlwZXMuU2VydmVyRGV0YWlsEiUKCFJvbGVMaXN0GAMgAygLMhMu" + "dHlwZXMuUm9sZUJhc2VJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeRoleBaseInfoReflection.Descriptor,
			TypeServerDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServerListAck), (MessageParser)(object)ServerListAck.Parser, new string[3] { "ServerId", "ServerList", "RoleList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
