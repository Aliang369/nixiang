using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeElementTrialFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeElementTrialFightResultReqReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkVsZW1lbnRUcmlhbEZpZ2h0UmVzdWx0UmVxLnByb3RvEgV0eXBl" + "cxobZW51bS5FU2tpbGxBdHRhY2tUeXBlLnByb3RvGht0eXBlLkJhdHRsZVJl" + "c3VsdEluZm8ucHJvdG8ikwEKGkVsZW1lbnRUcmlhbEZpZ2h0UmVzdWx0UmVx" + "EjUKCVRyaWFsVHlwZRgBIAEoDjIiLkVTa2lsbEF0dGFja1R5cGUuRVNraWxs" + "QXR0YWNrVHlwZRIPCgdMZXZlbElkGAIgASgFEi0KDEJhdHRsZVJlc3VsdBgD" + "IAEoCzIXLnR5cGVzLkJhdHRsZVJlc3VsdEluZm9CLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESkillAttackTypeReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ElementTrialFightResultReq), (MessageParser)(object)ElementTrialFightResultReq.Parser, new string[3] { "TrialType", "LevelId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
