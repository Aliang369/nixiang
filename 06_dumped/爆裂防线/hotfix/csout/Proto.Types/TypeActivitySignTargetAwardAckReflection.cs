using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeActivitySignTargetAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeActivitySignTargetAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkFjdGl2aXR5U2lnblRhcmdldEF3YXJkQWNrLnByb3RvEgV0eXBl" + "cxoXdHlwZS5BY3Rpdml0eUluZm8ucHJvdG8aFnR5cGUuQXdhcmREZXRhaWwu" + "cHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90byKVAQoaQWN0aXZpdHlTaWdu" + "VGFyZ2V0QXdhcmRBY2sSKQoMQWN0aXZpdHlJbmZvGAEgASgLMhMudHlwZXMu" + "QWN0aXZpdHlJbmZvEiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3YXJk" + "RGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hhbmdl" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeActivityInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ActivitySignTargetAwardAck), (MessageParser)(object)ActivitySignTargetAwardAck.Parser, new string[3] { "ActivityInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
