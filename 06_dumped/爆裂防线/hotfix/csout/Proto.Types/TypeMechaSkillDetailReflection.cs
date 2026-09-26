using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMechaSkillDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMechaSkillDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLk1lY2hhU2tpbGxEZXRhaWwucHJvdG8SBXR5cGVzImYKEE1lY2hh" + "U2tpbGxEZXRhaWwSDQoFSW5kZXgYASABKAUSDQoFTGV2ZWwYAiABKAUSDwoH" + "U2tpbGxJZBgDIAEoBRIPCgdTa2lsbENkGAQgASgCEhIKCkRhbWFnZVJhdGUY" + "BSABKAJCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MechaSkillDetail), (MessageParser)(object)MechaSkillDetail.Parser, new string[5] { "Index", "Level", "SkillId", "SkillCd", "DamageRate" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
