using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleTeamFootholdInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleTeamFootholdInfoReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkJhdHRsZVRlYW1Gb290aG9sZEluZm8ucHJvdG8SBXR5cGVzIkoK" + "FkJhdHRsZVRlYW1Gb290aG9sZEluZm8SEgoKRm9vdGhvbGRJZBgBIAEoBRIM" + "CgRTdGFyGAIgASgFEg4KBkRhbWFnZRgDIAEoAkIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleTeamFootholdInfo), (MessageParser)(object)BattleTeamFootholdInfo.Parser, new string[3] { "FootholdId", "Star", "Damage" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
