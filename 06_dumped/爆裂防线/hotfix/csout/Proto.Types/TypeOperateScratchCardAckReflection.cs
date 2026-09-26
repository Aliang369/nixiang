using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateScratchCardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateScratchCardAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLk9wZXJhdGVTY3JhdGNoQ2FyZEFjay5wcm90bxIFdHlwZXMaFXR5" + "cGUuRGF0YUNoYW5nZS5wcm90bxocdHlwZS5TY3JhdGNoQ2FyZERldGFpbC5w" + "cm90byJxChVPcGVyYXRlU2NyYXRjaENhcmRBY2sSMQoPU2NyYXRjaENhcmRM" + "aXN0GAEgAygLMhgudHlwZXMuU2NyYXRjaENhcmREZXRhaWwSJQoKRGF0YUNo" + "YW5nZRgCIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeScratchCardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateScratchCardAck), (MessageParser)(object)OperateScratchCardAck.Parser, new string[2] { "ScratchCardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
