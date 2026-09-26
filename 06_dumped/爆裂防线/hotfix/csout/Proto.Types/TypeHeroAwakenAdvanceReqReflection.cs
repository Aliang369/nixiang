using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroAwakenAdvanceReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenAdvanceReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkhlcm9Bd2FrZW5BZHZhbmNlUmVxLnByb3RvEgV0eXBlcxoVZW51" + "bS5FU2tpbGxUeXBlLnByb3RvIkEKFEhlcm9Bd2FrZW5BZHZhbmNlUmVxEikK" + "CVNraWxsVHlwZRgBIAEoDjIWLkVTa2lsbFR5cGUuRVNraWxsVHlwZUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenAdvanceReq), (MessageParser)(object)HeroAwakenAdvanceReq.Parser, new string[1] { "SkillType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
