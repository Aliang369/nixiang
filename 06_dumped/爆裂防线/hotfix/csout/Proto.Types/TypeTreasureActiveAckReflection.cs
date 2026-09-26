using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTreasureActiveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTreasureActiveAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlRyZWFzdXJlQWN0aXZlQWNrLnByb3RvEgV0eXBlcxoVdHlwZS5E" + "YXRhQ2hhbmdlLnByb3RvGhd0eXBlLlRyZWFzdXJlSW5mby5wcm90byJlChFU" + "cmVhc3VyZUFjdGl2ZUFjaxIpCgxUcmVhc3VyZUluZm8YASABKAsyEy50eXBl" + "cy5UcmVhc3VyZUluZm8SJQoKRGF0YUNoYW5nZRgCIAEoCzIRLnR5cGVzLkRh" + "dGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeTreasureInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TreasureActiveAck), (MessageParser)(object)TreasureActiveAck.Parser, new string[2] { "TreasureInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
