using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSuitUpgradeStarAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSuitUpgradeStarAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlN1aXRVcGdyYWRlU3RhckFjay5wcm90bxIFdHlwZXMaFXR5cGUu" + "RGF0YUNoYW5nZS5wcm90bxoZdHlwZS5QbGF5ZXJTdWl0SW5mby5wcm90byJk" + "ChJTdWl0VXBncmFkZVN0YXJBY2sSJwoIU3VpdEluZm8YASABKAsyFS50eXBl" + "cy5QbGF5ZXJTdWl0SW5mbxIlCgpEYXRhQ2hhbmdlGAIgASgLMhEudHlwZXMu" + "RGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePlayerSuitInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SuitUpgradeStarAck), (MessageParser)(object)SuitUpgradeStarAck.Parser, new string[2] { "SuitInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
