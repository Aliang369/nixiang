using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroTrialInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroTrialInfoReflection()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkhlcm9UcmlhbEluZm8ucHJvdG8SBXR5cGVzGhVlbnVtLkVTa2ls" + "bFR5cGUucHJvdG8aFnR5cGUuVHJpYWxEZXRhaWwucHJvdG8iyAEKDUhlcm9U" + "cmlhbEluZm8SKwoLQ3VycmVudFR5cGUYASABKA4yFi5FU2tpbGxUeXBlLkVT" + "a2lsbFR5cGUSFAoMQ3VycmVudExldmVsGAIgASgFEhEKCVNob3dFbmRUbRgD" + "IAEoAxIRCglSZWZyZXNoVG0YBCABKAMSEwoLQ29tYmF0Q291bnQYBSABKAUS" + "EgoKRGFpbHlDb3VudBgGIAEoBRIlCglUcmlhbExpc3QYByADKAsyEi50eXBl" + "cy5UcmlhbERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESkillTypeReflection.Descriptor,
			TypeTrialDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroTrialInfo), (MessageParser)(object)HeroTrialInfo.Parser, new string[7] { "CurrentType", "CurrentLevel", "ShowEndTm", "RefreshTm", "CombatCount", "DailyCount", "TrialList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
