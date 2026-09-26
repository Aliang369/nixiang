using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroTrialFightStartReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroTrialFightStartReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkhlcm9UcmlhbEZpZ2h0U3RhcnRSZXEucHJvdG8SBXR5cGVzGhVl" + "bnVtLkVTa2lsbFR5cGUucHJvdG8iVAoWSGVyb1RyaWFsRmlnaHRTdGFydFJl" + "cRIpCglUcmlhbFR5cGUYASABKA4yFi5FU2tpbGxUeXBlLkVTa2lsbFR5cGUS" + "DwoHTGV2ZWxJZBgCIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroTrialFightStartReq), (MessageParser)(object)HeroTrialFightStartReq.Parser, new string[2] { "TrialType", "LevelId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
