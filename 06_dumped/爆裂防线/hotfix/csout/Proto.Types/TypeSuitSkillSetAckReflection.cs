using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSuitSkillSetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSuitSkillSetAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLlN1aXRTa2lsbFNldEFjay5wcm90bxIFdHlwZXMaFXR5cGUuRGF0" + "YUNoYW5nZS5wcm90bxoZdHlwZS5QbGF5ZXJTdWl0SW5mby5wcm90byJhCg9T" + "dWl0U2tpbGxTZXRBY2sSJwoIU3VpdEluZm8YASABKAsyFS50eXBlcy5QbGF5" + "ZXJTdWl0SW5mbxIlCgpEYXRhQ2hhbmdlGAIgASgLMhEudHlwZXMuRGF0YUNo" + "YW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePlayerSuitInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SuitSkillSetAck), (MessageParser)(object)SuitSkillSetAck.Parser, new string[2] { "SuitInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
