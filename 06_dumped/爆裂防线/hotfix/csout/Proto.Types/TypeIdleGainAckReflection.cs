using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeIdleGainAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeIdleGainAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLklkbGVHYWluQWNrLnByb3RvEgV0eXBlcxoWdHlwZS5Bd2FyZERl" + "dGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhN0eXBlLklkbGVJ" + "bmZvLnByb3RvIn4KC0lkbGVHYWluQWNrEiEKCElkbGVJbmZvGAEgASgLMg8u" + "dHlwZXMuSWRsZUluZm8SJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdh" + "cmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFu" + "Z2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeIdleInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(IdleGainAck), (MessageParser)(object)IdleGainAck.Parser, new string[3] { "IdleInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
