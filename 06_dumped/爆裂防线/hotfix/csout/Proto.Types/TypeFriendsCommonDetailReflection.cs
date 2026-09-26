using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFriendsCommonDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsCommonDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkZyaWVuZHNDb21tb25EZXRhaWwucHJvdG8SBXR5cGVzGhd0eXBl" + "LlJvbGVCYXNlSW5mby5wcm90bxoYdHlwZS5Sb2xlTG9naW5JbmZvLnByb3Rv" + "Im0KE0ZyaWVuZHNDb21tb25EZXRhaWwSKQoMUm9sZUJhc2VJbmZvGAEgASgL" + "MhMudHlwZXMuUm9sZUJhc2VJbmZvEisKDVJvbGVMb2dpbkluZm8YAiABKAsy" + "FC50eXBlcy5Sb2xlTG9naW5JbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeRoleBaseInfoReflection.Descriptor,
			TypeRoleLoginInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsCommonDetail), (MessageParser)(object)FriendsCommonDetail.Parser, new string[2] { "RoleBaseInfo", "RoleLoginInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
