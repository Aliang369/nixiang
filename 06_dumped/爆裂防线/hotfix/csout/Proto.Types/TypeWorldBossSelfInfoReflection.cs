using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldBossSelfInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossSelfInfoReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLldvcmxkQm9zc1NlbGZJbmZvLnByb3RvEgV0eXBlcyK6AQoRV29y" + "bGRCb3NzU2VsZkluZm8SEwoLQ29tYmF0Q291bnQYASABKAUSEQoJQnV5Q29t" + "YmF0GAIgASgFEhIKCkRhaWx5Q291bnQYAyABKAUSDwoHTWF4SHVydBgEIAEo" + "AxIRCglTaG93U3RhcnQYBSABKAMSDwoHU2hvd0VuZBgGIAEoAxIPCgdTdGFy" + "dFRtGAcgASgDEg0KBUVuZFRtGAggASgDEhQKDE1vbnRoUmVmcmVzaBgJIAEo" + "A0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossSelfInfo), (MessageParser)(object)WorldBossSelfInfo.Parser, new string[9] { "CombatCount", "BuyCombat", "DailyCount", "MaxHurt", "ShowStart", "ShowEnd", "StartTm", "EndTm", "MonthRefresh" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
