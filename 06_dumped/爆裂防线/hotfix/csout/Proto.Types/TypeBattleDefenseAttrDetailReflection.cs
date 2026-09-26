using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleDefenseAttrDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleDefenseAttrDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkJhdHRsZURlZmVuc2VBdHRyRGV0YWlsLnByb3RvEgV0eXBlcxob" + "ZW51bS5FQXR0ckRlZmVuc2VUeXBlLnByb3RvIloKF0JhdHRsZURlZmVuc2VB" + "dHRyRGV0YWlsEjAKBFR5cGUYASABKA4yIi5FQXR0ckRlZmVuc2VUeXBlLkVB" + "dHRyRGVmZW5zZVR5cGUSDQoFVmFsdWUYAiABKAJCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEAttrDefenseTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleDefenseAttrDetail), (MessageParser)(object)BattleDefenseAttrDetail.Parser, new string[2] { "Type", "Value" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
