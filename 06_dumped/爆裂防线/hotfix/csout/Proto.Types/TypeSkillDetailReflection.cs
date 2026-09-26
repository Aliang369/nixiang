using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSkillDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSkillDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLlNraWxsRGV0YWlsLnByb3RvEgV0eXBlcxoVZW51bS5FU2tpbGxU" + "eXBlLnByb3RvIloKC1NraWxsRGV0YWlsEikKCVNraWxsVHlwZRgBIAEoDjIW" + "LkVTa2lsbFR5cGUuRVNraWxsVHlwZRIPCgdTa2lsbElkGAIgASgFEg8KB0hl" + "cm9UaWQYAyABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SkillDetail), (MessageParser)(object)SkillDetail.Parser, new string[3] { "SkillType", "SkillId", "HeroTid" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
