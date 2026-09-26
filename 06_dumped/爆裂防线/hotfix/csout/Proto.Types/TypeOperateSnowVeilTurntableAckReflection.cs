using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilTurntableAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilTurntableAckReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVTbm93VmVpbFR1cm50YWJsZUFjay5wcm90bxIFdHlw" + "ZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5w" + "cm90bxoZdHlwZS5Tbm93VmVpbERldGFpbC5wcm90byKnAQobT3BlcmF0ZVNu" + "b3dWZWlsVHVybnRhYmxlQWNrEg0KBUluZGV4GAEgASgFEisKDFNub3dWZWls" + "TGlzdBgCIAMoCzIVLnR5cGVzLlNub3dWZWlsRGV0YWlsEiUKCUF3YXJkTGlz" + "dBgDIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYBCAB" + "KAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeSnowVeilDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilTurntableAck), (MessageParser)(object)OperateSnowVeilTurntableAck.Parser, new string[4] { "Index", "SnowVeilList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
