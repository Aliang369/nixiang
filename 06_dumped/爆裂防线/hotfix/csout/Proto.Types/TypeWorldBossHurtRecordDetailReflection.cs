using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldBossHurtRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossHurtRecordDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLldvcmxkQm9zc0h1cnRSZWNvcmREZXRhaWwucHJvdG8SBXR5cGVz" + "Ght0eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJvdG8aF3R5cGUuUm9sZUJhc2VJ" + "bmZvLnByb3RvIpEBChlXb3JsZEJvc3NIdXJ0UmVjb3JkRGV0YWlsEikKDFJv" + "bGVCYXNlSW5mbxgBIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mbxItCgxCYXR0" + "bGVSZXN1bHQYAiABKAsyFy50eXBlcy5CYXR0bGVSZXN1bHRJbmZvEgwKBEh1" + "cnQYAyABKAMSDAoEVGltZRgEIAEoA0IuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleResultInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossHurtRecordDetail), (MessageParser)(object)WorldBossHurtRecordDetail.Parser, new string[4] { "RoleBaseInfo", "BattleResult", "Hurt", "Time" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
