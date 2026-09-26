using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeElementTrialFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeElementTrialFightStartAckReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkVsZW1lbnRUcmlhbEZpZ2h0U3RhcnRBY2sucHJvdG8SBXR5cGVz" + "GhtlbnVtLkVTa2lsbEF0dGFja1R5cGUucHJvdG8aFXR5cGUuQmF0dGxlSW5m" + "by5wcm90byKKAQoZRWxlbWVudFRyaWFsRmlnaHRTdGFydEFjaxI1CglUcmlh" + "bFR5cGUYASABKA4yIi5FU2tpbGxBdHRhY2tUeXBlLkVTa2lsbEF0dGFja1R5" + "cGUSDwoHTGV2ZWxJZBgCIAEoBRIlCgpCYXR0bGVJbmZvGAMgASgLMhEudHlw" + "ZXMuQmF0dGxlSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESkillAttackTypeReflection.Descriptor,
			TypeBattleInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ElementTrialFightStartAck), (MessageParser)(object)ElementTrialFightStartAck.Parser, new string[3] { "TrialType", "LevelId", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
