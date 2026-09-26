using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonProduceItemReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonProduceItemReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlYXNvblByb2R1Y2VJdGVtLnByb3RvEgV0eXBlcxoZZW51bS5F" + "U2Vhc29uUmVzVHlwZS5wcm90byJ1ChFTZWFzb25Qcm9kdWNlSXRlbRIRCglS" + "ZXNOb2RlSWQYASABKAUSLwoHUmVzVHlwZRgCIAEoDjIeLkVTZWFzb25SZXNU" + "eXBlLkVTZWFzb25SZXNUeXBlEgwKBEdhaW4YAyABKAUSDgoGQWN0aXZlGAQg" + "ASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonResTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonProduceItem), (MessageParser)(object)SeasonProduceItem.Parser, new string[4] { "ResNodeId", "ResType", "Gain", "Active" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
