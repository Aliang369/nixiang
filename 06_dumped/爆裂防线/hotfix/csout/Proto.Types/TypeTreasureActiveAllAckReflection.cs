using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTreasureActiveAllAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTreasureActiveAllAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlRyZWFzdXJlQWN0aXZlQWxsQWNrLnByb3RvEgV0eXBlcxoVdHlw" + "ZS5EYXRhQ2hhbmdlLnByb3RvGhd0eXBlLlRyZWFzdXJlSW5mby5wcm90byJo" + "ChRUcmVhc3VyZUFjdGl2ZUFsbEFjaxIpCgxUcmVhc3VyZUluZm8YASABKAsy" + "Ey50eXBlcy5UcmVhc3VyZUluZm8SJQoKRGF0YUNoYW5nZRgCIAEoCzIRLnR5" + "cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeTreasureInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TreasureActiveAllAck), (MessageParser)(object)TreasureActiveAllAck.Parser, new string[2] { "TreasureInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
