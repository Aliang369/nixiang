using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePeerageUpgradeTaskAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePeerageUpgradeTaskAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlBlZXJhZ2VVcGdyYWRlVGFza0Fjay5wcm90bxIFdHlwZXMaFnR5" + "cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoW" + "dHlwZS5QZWVyYWdlSW5mby5wcm90byKOAQoVUGVlcmFnZVVwZ3JhZGVUYXNr" + "QWNrEicKC1BlZXJhZ2VJbmZvGAEgASgLMhIudHlwZXMuUGVlcmFnZUluZm8S" + "JQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0" + "YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypePeerageInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PeerageUpgradeTaskAck), (MessageParser)(object)PeerageUpgradeTaskAck.Parser, new string[3] { "PeerageInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
