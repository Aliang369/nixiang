using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTreasureUpgradeLevelAllAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTreasureUpgradeLevelAllAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLlRyZWFzdXJlVXBncmFkZUxldmVsQWxsQWNrLnByb3RvEgV0eXBl" + "cxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhd0eXBlLlRyZWFzdXJlSW5mby5w" + "cm90byJuChpUcmVhc3VyZVVwZ3JhZGVMZXZlbEFsbEFjaxIpCgxUcmVhc3Vy" + "ZUluZm8YASABKAsyEy50eXBlcy5UcmVhc3VyZUluZm8SJQoKRGF0YUNoYW5n" + "ZRgCIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeTreasureInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TreasureUpgradeLevelAllAck), (MessageParser)(object)TreasureUpgradeLevelAllAck.Parser, new string[2] { "TreasureInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
