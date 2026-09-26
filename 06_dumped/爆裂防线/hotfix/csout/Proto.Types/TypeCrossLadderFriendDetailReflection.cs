using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderFriendDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderFriendDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkNyb3NzTGFkZGVyRnJpZW5kRGV0YWlsLnByb3RvEgV0eXBlcxoX" + "dHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8iVgoXQ3Jvc3NMYWRkZXJGcmllbmRE" + "ZXRhaWwSKQoMUm9sZUJhc2VJbmZvGAEgASgLMhMudHlwZXMuUm9sZUJhc2VJ" + "bmZvEhAKCExhZGRlcklkGAIgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderFriendDetail), (MessageParser)(object)CrossLadderFriendDetail.Parser, new string[2] { "RoleBaseInfo", "LadderId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
