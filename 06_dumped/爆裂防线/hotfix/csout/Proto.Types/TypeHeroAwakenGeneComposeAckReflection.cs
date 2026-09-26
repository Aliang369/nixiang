using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHeroAwakenGeneComposeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenGeneComposeAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkhlcm9Bd2FrZW5HZW5lQ29tcG9zZUFjay5wcm90bxIFdHlwZXMa" + "FXR5cGUuRGF0YUNoYW5nZS5wcm90bxoZdHlwZS5IZXJvQXdha2VuSW5mby5w" + "cm90byJwChhIZXJvQXdha2VuR2VuZUNvbXBvc2VBY2sSLQoOSGVyb0F3YWtl" + "bkluZm8YASABKAsyFS50eXBlcy5IZXJvQXdha2VuSW5mbxIlCgpEYXRhQ2hh" + "bmdlGAIgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeHeroAwakenInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenGeneComposeAck), (MessageParser)(object)HeroAwakenGeneComposeAck.Parser, new string[2] { "HeroAwakenInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
