using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonCloseAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonCloseAckReflection()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLldvcmxkRHVuZ2VvbkNsb3NlQWNrLnByb3RvEgV0eXBlcxoVdHlw" + "ZS5EYXRhQ2hhbmdlLnByb3RvGht0eXBlLkR1bmdlb25Bd2FyZEluZm8ucHJv" + "dG8aGnR5cGUuRHVuZ2VvbkJhc2VJbmZvLnByb3RvGhl0eXBlLkR1bmdlb25N" + "YXBJbmZvLnByb3RvGhx0eXBlLkR1bmdlb25SZXN1bHRJbmZvLnByb3RvGhR0" + "eXBlLldvcmxkSW5mby5wcm90byKqAgoUV29ybGREdW5nZW9uQ2xvc2VBY2sS" + "MwoRRHVuZ2VvblJlc3VsdEluZm8YASABKAsyGC50eXBlcy5EdW5nZW9uUmVz" + "dWx0SW5mbxIvCg9EdW5nZW9uQmFzZUluZm8YAiABKAsyFi50eXBlcy5EdW5n" + "ZW9uQmFzZUluZm8SLQoORHVuZ2Vvbk1hcEluZm8YAyABKAsyFS50eXBlcy5E" + "dW5nZW9uTWFwSW5mbxIxChBEdW5nZW9uQXdhcmRJbmZvGAQgASgLMhcudHlw" + "ZXMuRHVuZ2VvbkF3YXJkSW5mbxIjCglXb3JsZEluZm8YBSABKAsyEC50eXBl" + "cy5Xb3JsZEluZm8SJQoKRGF0YUNoYW5nZRgGIAEoCzIRLnR5cGVzLkRhdGFD" + "aGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			TypeDataChangeReflection.Descriptor,
			TypeDungeonAwardInfoReflection.Descriptor,
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor,
			TypeDungeonResultInfoReflection.Descriptor,
			TypeWorldInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonCloseAck), (MessageParser)(object)WorldDungeonCloseAck.Parser, new string[6] { "DungeonResultInfo", "DungeonBaseInfo", "DungeonMapInfo", "DungeonAwardInfo", "WorldInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
