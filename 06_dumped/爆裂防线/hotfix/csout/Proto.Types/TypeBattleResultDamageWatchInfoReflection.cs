using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleResultDamageWatchInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleResultDamageWatchInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLkJhdHRsZVJlc3VsdERhbWFnZVdhdGNoSW5mby5wcm90bxIFdHlw" + "ZXMiewobQmF0dGxlUmVzdWx0RGFtYWdlV2F0Y2hJbmZvEhIKCkRhbWFnZUlu" + "aXQYASABKAUSEwoLRGFtYWdlRmluYWwYAiABKAUSDwoHSHVydE1pbhgDIAEo" + "AxIPCgdIdXJ0TWF4GAQgASgDEhEKCU1vbnN0ZXJJZBgFIAEoBUIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleResultDamageWatchInfo), (MessageParser)(object)BattleResultDamageWatchInfo.Parser, new string[5] { "DamageInit", "DamageFinal", "HurtMin", "HurtMax", "MonsterId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
