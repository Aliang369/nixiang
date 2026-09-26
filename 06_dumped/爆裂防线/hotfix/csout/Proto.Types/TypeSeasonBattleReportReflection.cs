using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonBattleReportReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonBattleReportReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlNlYXNvbkJhdHRsZVJlcG9ydC5wcm90bxIFdHlwZXMaHGVudW0u" + "RVNlYXNvbkJhdHRsZVR5cGUucHJvdG8iogEKElNlYXNvbkJhdHRsZVJlcG9y" + "dBIMCgRVdWlkGAEgASgJEhEKCUF0a1JvbGVJZBgCIAEoAxIRCglEZWZSb2xl" + "SWQYAyABKAMSDgoGQ2l0eUlkGAQgASgFEg4KBldpbm5lchgFIAEoBRI4CgpC" + "YXR0bGVUeXBlGAYgASgOMiQuRVNlYXNvbkJhdHRsZVR5cGUuRVNlYXNvbkJh" + "dHRsZVR5cGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonBattleTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonBattleReport), (MessageParser)(object)SeasonBattleReport.Parser, new string[6] { "Uuid", "AtkRoleId", "DefRoleId", "CityId", "Winner", "BattleType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
