using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionInfoReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLkxlZ2lvbkluZm8ucHJvdG8SBXR5cGVzGhl0eXBlLkxlZ2lvbkJh" + "c2VJbmZvLnByb3RvGhl0eXBlLkxlZ2lvbkJvc3NJbmZvLnByb3RvGhh0eXBl" + "LkxlZ2lvblNldEluZm8ucHJvdG8ilwEKCkxlZ2lvbkluZm8SLQoOTGVnaW9u" + "QmFzZUluZm8YASABKAsyFS50eXBlcy5MZWdpb25CYXNlSW5mbxIrCg1MZWdp" + "b25TZXRJbmZvGAIgASgLMhQudHlwZXMuTGVnaW9uU2V0SW5mbxItCg5MZWdp" + "b25Cb3NzSW5mbxgDIAEoCzIVLnR5cGVzLkxlZ2lvbkJvc3NJbmZvQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeLegionBaseInfoReflection.Descriptor,
			TypeLegionBossInfoReflection.Descriptor,
			TypeLegionSetInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionInfo), (MessageParser)(object)LegionInfo.Parser, new string[3] { "LegionBaseInfo", "LegionSetInfo", "LegionBossInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
