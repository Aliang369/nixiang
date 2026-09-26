using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionSelfBossReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionSelfBossReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkxlZ2lvblNlbGZCb3NzLnByb3RvEgV0eXBlcyJwCg5MZWdpb25T" + "ZWxmQm9zcxITCgtDb21iYXRDb3VudBgBIAEoBRIRCglCdXlDb21iYXQYAiAB" + "KAUSEgoKRGFpbHlDb3VudBgDIAEoBRIRCglQYXNzQXdhcmQYBCADKAUSDwoH" + "TWF4S2lsbBgFIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionSelfBoss), (MessageParser)(object)LegionSelfBoss.Parser, new string[5] { "CombatCount", "BuyCombat", "DailyCount", "PassAward", "MaxKill" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
