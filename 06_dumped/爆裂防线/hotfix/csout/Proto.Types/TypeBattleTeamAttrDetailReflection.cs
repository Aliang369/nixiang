using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleTeamAttrDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleTeamAttrDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkJhdHRsZVRlYW1BdHRyRGV0YWlsLnByb3RvEgV0eXBlcxoYZW51" + "bS5FQXR0clRlYW1UeXBlLnByb3RvIlEKFEJhdHRsZVRlYW1BdHRyRGV0YWls" + "EioKBFR5cGUYASABKA4yHC5FQXR0clRlYW1UeXBlLkVBdHRyVGVhbVR5cGUS" + "DQoFVmFsdWUYAiABKAJCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEAttrTeamTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleTeamAttrDetail), (MessageParser)(object)BattleTeamAttrDetail.Parser, new string[2] { "Type", "Value" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
