using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionNameChangeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionNameChangeAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkxlZ2lvbk5hbWVDaGFuZ2VBY2sucHJvdG8SBXR5cGVzGhV0eXBl" + "LkRhdGFDaGFuZ2UucHJvdG8aGXR5cGUuTGVnaW9uQmFzZUluZm8ucHJvdG8i" + "awoTTGVnaW9uTmFtZUNoYW5nZUFjaxItCg5MZWdpb25CYXNlSW5mbxgBIAEo" + "CzIVLnR5cGVzLkxlZ2lvbkJhc2VJbmZvEiUKCkRhdGFDaGFuZ2UYAiABKAsy" + "ES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeLegionBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionNameChangeAck), (MessageParser)(object)LegionNameChangeAck.Parser, new string[2] { "LegionBaseInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
