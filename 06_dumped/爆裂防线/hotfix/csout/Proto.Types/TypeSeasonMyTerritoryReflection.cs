using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonMyTerritoryReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonMyTerritoryReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlYXNvbk15VGVycml0b3J5LnByb3RvEgV0eXBlcxoZZW51bS5F" + "U2Vhc29uUmVzVHlwZS5wcm90byKeAQoRU2Vhc29uTXlUZXJyaXRvcnkSEQoJ" + "UmVzTm9kZUlkGAEgASgFEi8KB1Jlc1R5cGUYAiABKA4yHi5FU2Vhc29uUmVz" + "VHlwZS5FU2Vhc29uUmVzVHlwZRINCgVMZXZlbBgDIAEoBRIOCgZDaXR5SWQY" + "BCABKAUSFgoOUHJvZHVjZVBlckhvdXIYBSABKAUSDgoGQWN0aXZlGAYgASgF" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonResTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonMyTerritory), (MessageParser)(object)SeasonMyTerritory.Parser, new string[6] { "ResNodeId", "ResType", "Level", "CityId", "ProducePerHour", "Active" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
