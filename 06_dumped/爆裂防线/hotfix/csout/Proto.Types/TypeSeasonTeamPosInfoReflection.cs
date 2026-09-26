using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonTeamPosInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonTeamPosInfoReflection()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlYXNvblRlYW1Qb3NJbmZvLnByb3RvEgV0eXBlcxoXZW51bS5F" + "U2Vhc29uRm9yY2UucHJvdG8aHnR5cGUuU2Vhc29uVGVhbU1hcmNoSW5mby5w" + "cm90byLaAQoRU2Vhc29uVGVhbVBvc0luZm8SDgoGUm9sZUlkGAEgASgDEg4K" + "BlRlYW1JZBgCIAEoBRIrCgdGb3JjZUlkGAMgASgOMhouRVNlYXNvbkZvcmNl" + "LkVTZWFzb25Gb3JjZRINCgVTdGF0ZRgEIAEoBRIOCgZDaXR5SWQYBSABKAUS" + "DAoEUG9zWBgGIAEoAhIMCgRQb3NZGAcgASgCEhIKCkR1cmFiaWxpdHkYCCAB" + "KAUSKQoFTWFyY2gYCSABKAsyGi50eXBlcy5TZWFzb25UZWFtTWFyY2hJbmZv" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESeasonForceReflection.Descriptor,
			TypeSeasonTeamMarchInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonTeamPosInfo), (MessageParser)(object)SeasonTeamPosInfo.Parser, new string[9] { "RoleId", "TeamId", "ForceId", "State", "CityId", "PosX", "PosY", "Durability", "March" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
