using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePvpSetModStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePvpSetModStartAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlB2cFNldE1vZFN0YXJ0QWNrLnByb3RvEgV0eXBlcxoVdHlwZS5E" + "YXRhQ2hhbmdlLnByb3RvGhJ0eXBlLlB2cEluZm8ucHJvdG8iWwoRUHZwU2V0" + "TW9kU3RhcnRBY2sSHwoHUHZwSW5mbxgBIAEoCzIOLnR5cGVzLlB2cEluZm8S" + "JQoKRGF0YUNoYW5nZRgCIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePvpInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PvpSetModStartAck), (MessageParser)(object)PvpSetModStartAck.Parser, new string[2] { "PvpInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
