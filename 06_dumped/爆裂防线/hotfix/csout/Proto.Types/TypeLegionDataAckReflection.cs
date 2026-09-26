using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionDataAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkxlZ2lvbkRhdGFBY2sucHJvdG8SBXR5cGVzGhV0eXBlLkxlZ2lv" + "bkluZm8ucHJvdG8aGXR5cGUuTGVnaW9uU2VsZkluZm8ucHJvdG8iZQoNTGVn" + "aW9uRGF0YUFjaxItCg5MZWdpb25TZWxmSW5mbxgBIAEoCzIVLnR5cGVzLkxl" + "Z2lvblNlbGZJbmZvEiUKCkxlZ2lvbkluZm8YAiABKAsyES50eXBlcy5MZWdp" + "b25JbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLegionInfoReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionDataAck), (MessageParser)(object)LegionDataAck.Parser, new string[2] { "LegionSelfInfo", "LegionInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
