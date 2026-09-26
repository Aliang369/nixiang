using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTreasureUpgradeLevelAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTreasureUpgradeLevelAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLlRyZWFzdXJlVXBncmFkZUxldmVsQWNrLnByb3RvEgV0eXBlcxoV" + "dHlwZS5EYXRhQ2hhbmdlLnByb3RvGhd0eXBlLlRyZWFzdXJlSW5mby5wcm90" + "byJrChdUcmVhc3VyZVVwZ3JhZGVMZXZlbEFjaxIpCgxUcmVhc3VyZUluZm8Y" + "ASABKAsyEy50eXBlcy5UcmVhc3VyZUluZm8SJQoKRGF0YUNoYW5nZRgCIAEo" + "CzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeTreasureInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TreasureUpgradeLevelAck), (MessageParser)(object)TreasureUpgradeLevelAck.Parser, new string[2] { "TreasureInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
