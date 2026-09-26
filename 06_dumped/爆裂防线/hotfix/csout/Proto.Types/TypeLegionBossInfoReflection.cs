using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionBossInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionBossInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkxlZ2lvbkJvc3NJbmZvLnByb3RvEgV0eXBlcxoVZW51bS5FQm9v" + "bFN0YXRlLnByb3RvIpIBCg5MZWdpb25Cb3NzSW5mbxIOCgZCb3NzSWQYASAB" + "KAUSDwoHU3RhcnRUbRgCIAEoAxINCgVFbmRUbRgDIAEoAxIRCglLaWxsVG90" + "YWwYBCABKAUSEgoKVGFyZ2V0TGlzdBgFIAMoBRIpCglQYXNzU3RhdGUYBiAB" + "KA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGVCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionBossInfo), (MessageParser)(object)LegionBossInfo.Parser, new string[6] { "BossId", "StartTm", "EndTm", "KillTotal", "TargetList", "PassState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
