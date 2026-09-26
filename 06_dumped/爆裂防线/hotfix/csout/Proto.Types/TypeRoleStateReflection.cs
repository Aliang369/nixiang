using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRoleStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleStateReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChR0eXBlLlJvbGVTdGF0ZS5wcm90bxIFdHlwZXMaFWVudW0uRVJvbGVTdGF0" + "ZS5wcm90byJ1CglSb2xlU3RhdGUSEAoIU2VydmVySWQYASABKAUSDgoGUm9s" + "ZUlkGAIgASgDEhAKCFJvbGVOYW1lGAMgASgJEg0KBUxldmVsGAQgASgFEiUK" + "BVN0YXRlGAUgASgOMhYuRVJvbGVTdGF0ZS5FUm9sZVN0YXRlQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumERoleStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleState), (MessageParser)(object)RoleState.Parser, new string[5] { "ServerId", "RoleId", "RoleName", "Level", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
