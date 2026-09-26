using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeGunSetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGunSetAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChR0eXBlLkd1blNldEFjay5wcm90bxIFdHlwZXMaFXR5cGUuRGF0YUNoYW5n" + "ZS5wcm90bxoYdHlwZS5QbGF5ZXJHdW5JbmZvLnByb3RvIlkKCUd1blNldEFj" + "axIlCgdHdW5JbmZvGAEgASgLMhQudHlwZXMuUGxheWVyR3VuSW5mbxIlCgpE" + "YXRhQ2hhbmdlGAIgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePlayerGunInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GunSetAck), (MessageParser)(object)GunSetAck.Parser, new string[2] { "GunInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
