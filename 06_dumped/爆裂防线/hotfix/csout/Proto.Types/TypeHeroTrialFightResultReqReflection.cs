using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroTrialFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroTrialFightResultReqReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkhlcm9UcmlhbEZpZ2h0UmVzdWx0UmVxLnByb3RvEgV0eXBlcxoV" + "ZW51bS5FU2tpbGxUeXBlLnByb3RvGht0eXBlLkJhdHRsZVJlc3VsdEluZm8u" + "cHJvdG8ihAEKF0hlcm9UcmlhbEZpZ2h0UmVzdWx0UmVxEikKCVRyaWFsVHlw" + "ZRgBIAEoDjIWLkVTa2lsbFR5cGUuRVNraWxsVHlwZRIPCgdMZXZlbElkGAIg" + "ASgFEi0KDEJhdHRsZVJlc3VsdBgDIAEoCzIXLnR5cGVzLkJhdHRsZVJlc3Vs" + "dEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESkillTypeReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroTrialFightResultReq), (MessageParser)(object)HeroTrialFightResultReq.Parser, new string[3] { "TrialType", "LevelId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
