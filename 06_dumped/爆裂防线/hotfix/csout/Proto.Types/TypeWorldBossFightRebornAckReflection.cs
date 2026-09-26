using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWorldBossFightRebornAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossFightRebornAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLldvcmxkQm9zc0ZpZ2h0UmVib3JuQWNrLnByb3RvEgV0eXBlcxoV" + "ZW51bS5FQm9vbFN0YXRlLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8i" + "dgoXV29ybGRCb3NzRmlnaHRSZWJvcm5BY2sSDAoEVVVJRBgBIAEoBRImCgZS" + "ZWJvcm4YAiABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGUSJQoKRGF0YUNo" + "YW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossFightRebornAck), (MessageParser)(object)WorldBossFightRebornAck.Parser, new string[3] { "UUID", "Reborn", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
