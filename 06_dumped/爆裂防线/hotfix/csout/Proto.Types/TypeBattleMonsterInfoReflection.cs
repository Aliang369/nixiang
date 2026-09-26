using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleMonsterInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleMonsterInfoReflection()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkJhdHRsZU1vbnN0ZXJJbmZvLnByb3RvEgV0eXBlcxoedHlwZS5C" + "YXR0bGVNb25zdGVyRGV0YWlsLnByb3RvGiF0eXBlLkJhdHRsZU1vbnN0ZXJF" + "eHRyYUluZm8ucHJvdG8aHnR5cGUuQmF0dGxlTW9uc3RlckZhY3Rvci5wcm90" + "bxojdHlwZS5CYXR0bGVNb25zdGVyUm91bmREZXRhaWwucHJvdG8i3QEKEUJh" + "dHRsZU1vbnN0ZXJJbmZvEjIKCVJvdW5kTGlzdBgBIAMoCzIfLnR5cGVzLkJh" + "dHRsZU1vbnN0ZXJSb3VuZERldGFpbBIvCgtNb25zdGVyTGlzdBgCIAMoCzIa" + "LnR5cGVzLkJhdHRsZU1vbnN0ZXJEZXRhaWwSMQoNTW9uc3RlckZhY3RvchgD" + "IAEoCzIaLnR5cGVzLkJhdHRsZU1vbnN0ZXJGYWN0b3ISMAoJRXh0cmFJbmZv" + "GAQgASgLMh0udHlwZXMuQmF0dGxlTW9uc3RlckV4dHJhSW5mb0IuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeBattleMonsterDetailReflection.Descriptor,
			TypeBattleMonsterExtraInfoReflection.Descriptor,
			TypeBattleMonsterFactorReflection.Descriptor,
			TypeBattleMonsterRoundDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleMonsterInfo), (MessageParser)(object)BattleMonsterInfo.Parser, new string[4] { "RoundList", "MonsterList", "MonsterFactor", "ExtraInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
