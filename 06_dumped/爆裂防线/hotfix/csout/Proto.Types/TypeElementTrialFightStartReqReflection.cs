using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeElementTrialFightStartReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeElementTrialFightStartReqReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkVsZW1lbnRUcmlhbEZpZ2h0U3RhcnRSZXEucHJvdG8SBXR5cGVz" + "GhtlbnVtLkVTa2lsbEF0dGFja1R5cGUucHJvdG8iYwoZRWxlbWVudFRyaWFs" + "RmlnaHRTdGFydFJlcRI1CglUcmlhbFR5cGUYASABKA4yIi5FU2tpbGxBdHRh" + "Y2tUeXBlLkVTa2lsbEF0dGFja1R5cGUSDwoHTGV2ZWxJZBgCIAEoBUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillAttackTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ElementTrialFightStartReq), (MessageParser)(object)ElementTrialFightStartReq.Parser, new string[2] { "TrialType", "LevelId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
