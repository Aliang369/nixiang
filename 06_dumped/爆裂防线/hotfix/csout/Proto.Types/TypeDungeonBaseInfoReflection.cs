using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDungeonBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDungeonBaseInfoReflection()
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkR1bmdlb25CYXNlSW5mby5wcm90bxIFdHlwZXMaFWVudW0uRVBh" + "c3NTdGF0ZS5wcm90bxoedHlwZS5EdW5nZW9uUm9sZVNpdGVJbmZvLnByb3Rv" + "GiJ0eXBlLkR1bmdlb25UcmlnZ2VyRXZlbnRJbmZvLnByb3RvGhZ0eXBlLlNr" + "aWxsRGV0YWlsLnByb3RvIucCCg9EdW5nZW9uQmFzZUluZm8SEQoJRHVuZ2Vv" + "bklkGAEgASgFEikKCVBhc3NTdGF0ZRgCIAEoDjIWLkVQYXNzU3RhdGUuRVBh" + "c3NTdGF0ZRIRCglSaW90VmFsdWUYAyABKAUSEgoKRXZlbnRDb3VudBgEIAEo" + "BRIUCgxUcmlnZ2VyQ291bnQYBSABKAUSEQoJS2lsbENvdW50GAYgASgFEhIK" + "CkRlZmVuc2VNYXgYByABKAUSEgoKRGVmZW5zZVBjdBgIIAEoAhIPCgdNb2RM" + "aXN0GAkgAygFEiUKCVNraWxsTGlzdBgKIAMoCzISLnR5cGVzLlNraWxsRGV0" + "YWlsEjAKDFJvbGVTaXRlSW5mbxgLIAEoCzIaLnR5cGVzLkR1bmdlb25Sb2xl" + "U2l0ZUluZm8SNAoMVHJpZ2dlckV2ZW50GAwgASgLMh4udHlwZXMuRHVuZ2Vv" + "blRyaWdnZXJFdmVudEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumEPassStateReflection.Descriptor,
			TypeDungeonRoleSiteInfoReflection.Descriptor,
			TypeDungeonTriggerEventInfoReflection.Descriptor,
			TypeSkillDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DungeonBaseInfo), (MessageParser)(object)DungeonBaseInfo.Parser, new string[12]
			{
				"DungeonId", "PassState", "RiotValue", "EventCount", "TriggerCount", "KillCount", "DefenseMax", "DefensePct", "ModList", "SkillList",
				"RoleSiteInfo", "TriggerEvent"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
