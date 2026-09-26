using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTruckBuyRobAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckBuyRobAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlRydWNrQnV5Um9iQWNrLnByb3RvEgV0eXBlcxoVdHlwZS5EYXRh" + "Q2hhbmdlLnByb3RvGhh0eXBlLlRydWNrU2VsZkluZm8ucHJvdG8iZAoOVHJ1" + "Y2tCdXlSb2JBY2sSKwoNVHJ1Y2tTZWxmSW5mbxgBIAEoCzIULnR5cGVzLlRy" + "dWNrU2VsZkluZm8SJQoKRGF0YUNoYW5nZRgCIAEoCzIRLnR5cGVzLkRhdGFD" + "aGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeTruckSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckBuyRobAck), (MessageParser)(object)TruckBuyRobAck.Parser, new string[2] { "TruckSelfInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
