using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleMonsterAttrDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleMonsterAttrDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkJhdHRsZU1vbnN0ZXJBdHRyRGV0YWlsLnByb3RvEgV0eXBlcxob" + "ZW51bS5FQXR0ck1vbnN0ZXJUeXBlLnByb3RvIloKF0JhdHRsZU1vbnN0ZXJB" + "dHRyRGV0YWlsEjAKBFR5cGUYASABKA4yIi5FQXR0ck1vbnN0ZXJUeXBlLkVB" + "dHRyTW9uc3RlclR5cGUSDQoFVmFsdWUYAiABKAJCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEAttrMonsterTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleMonsterAttrDetail), (MessageParser)(object)BattleMonsterAttrDetail.Parser, new string[2] { "Type", "Value" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
