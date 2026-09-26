using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroTrialFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroTrialFightStartAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkhlcm9UcmlhbEZpZ2h0U3RhcnRBY2sucHJvdG8SBXR5cGVzGhVl" + "bnVtLkVTa2lsbFR5cGUucHJvdG8aFXR5cGUuQmF0dGxlSW5mby5wcm90byJ7" + "ChZIZXJvVHJpYWxGaWdodFN0YXJ0QWNrEikKCVRyaWFsVHlwZRgBIAEoDjIW" + "LkVTa2lsbFR5cGUuRVNraWxsVHlwZRIPCgdMZXZlbElkGAIgASgFEiUKCkJh" + "dHRsZUluZm8YAyABKAsyES50eXBlcy5CYXR0bGVJbmZvQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESkillTypeReflection.Descriptor,
			TypeBattleInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroTrialFightStartAck), (MessageParser)(object)HeroTrialFightStartAck.Parser, new string[3] { "TrialType", "LevelId", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
