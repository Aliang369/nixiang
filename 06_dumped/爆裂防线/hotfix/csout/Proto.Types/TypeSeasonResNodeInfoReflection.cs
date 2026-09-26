using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonResNodeInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonResNodeInfoReflection()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlYXNvblJlc05vZGVJbmZvLnByb3RvEgV0eXBlcxoXZW51bS5F" + "U2Vhc29uRm9yY2UucHJvdG8aGWVudW0uRVNlYXNvblJlc1R5cGUucHJvdG8i" + "0AEKEVNlYXNvblJlc05vZGVJbmZvEhEKCVJlc05vZGVJZBgBIAEoBRIvCgdS" + "ZXNUeXBlGAIgASgOMh4uRVNlYXNvblJlc1R5cGUuRVNlYXNvblJlc1R5cGUS" + "DQoFTGV2ZWwYAyABKAUSEwoLT3duZXJSb2xlSWQYBCABKAUSKwoHRm9yY2VJ" + "ZBgFIAEoDjIaLkVTZWFzb25Gb3JjZS5FU2Vhc29uRm9yY2USFgoOUHJvZHVj" + "ZVBlckhvdXIYBiABKAUSDgoGQWN0aXZlGAcgASgFQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESeasonForceReflection.Descriptor,
			EnumESeasonResTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonResNodeInfo), (MessageParser)(object)SeasonResNodeInfo.Parser, new string[7] { "ResNodeId", "ResType", "Level", "OwnerRoleId", "ForceId", "ProducePerHour", "Active" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
