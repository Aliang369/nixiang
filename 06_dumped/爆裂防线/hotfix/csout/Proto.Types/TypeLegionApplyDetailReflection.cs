using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionApplyDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionApplyDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkxlZ2lvbkFwcGx5RGV0YWlsLnByb3RvEgV0eXBlcxoXdHlwZS5S" + "b2xlQmFzZUluZm8ucHJvdG8iTwoRTGVnaW9uQXBwbHlEZXRhaWwSKQoMUm9s" + "ZUJhc2VJbmZvGAEgASgLMhMudHlwZXMuUm9sZUJhc2VJbmZvEg8KB0FwcGx5" + "VG0YAiABKANCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionApplyDetail), (MessageParser)(object)LegionApplyDetail.Parser, new string[2] { "RoleBaseInfo", "ApplyTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
