using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeGunUpgradeStarAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGunUpgradeStarAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkd1blVwZ3JhZGVTdGFyQWNrLnByb3RvEgV0eXBlcxoVdHlwZS5E" + "YXRhQ2hhbmdlLnByb3RvGhh0eXBlLlBsYXllckd1bkluZm8ucHJvdG8iYQoR" + "R3VuVXBncmFkZVN0YXJBY2sSJQoHR3VuSW5mbxgBIAEoCzIULnR5cGVzLlBs" + "YXllckd1bkluZm8SJQoKRGF0YUNoYW5nZRgCIAEoCzIRLnR5cGVzLkRhdGFD" + "aGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePlayerGunInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GunUpgradeStarAck), (MessageParser)(object)GunUpgradeStarAck.Parser, new string[2] { "GunInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
