using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeFriendsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsDetailReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkZyaWVuZHNEZXRhaWwucHJvdG8SBXR5cGVzGhVlbnVtLkVCb29s" + "U3RhdGUucHJvdG8aJGVudW0uRUZyaWVuZEFjdGlvbkNvbGxlY3RTdGF0ZS5w" + "cm90bxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8aGHR5cGUuUm9sZUxvZ2lu" + "SW5mby5wcm90byLaAQoNRnJpZW5kc0RldGFpbBIpCgxSb2xlQmFzZUluZm8Y" + "ASABKAsyEy50eXBlcy5Sb2xlQmFzZUluZm8SKwoNUm9sZUxvZ2luSW5mbxgC" + "IAEoCzIULnR5cGVzLlJvbGVMb2dpbkluZm8SRgoIQWN0aW9uTWUYAyABKA4y" + "NC5FRnJpZW5kQWN0aW9uQ29sbGVjdFN0YXRlLkVGcmllbmRBY3Rpb25Db2xs" + "ZWN0U3RhdGUSKQoJQWN0aW9uSGltGAQgASgOMhYuRUJvb2xTdGF0ZS5FQm9v" + "bFN0YXRlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumEFriendActionCollectStateReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor,
			TypeRoleLoginInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsDetail), (MessageParser)(object)FriendsDetail.Parser, new string[4] { "RoleBaseInfo", "RoleLoginInfo", "ActionMe", "ActionHim" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
