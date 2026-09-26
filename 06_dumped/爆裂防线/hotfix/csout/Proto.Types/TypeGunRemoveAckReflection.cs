using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeGunRemoveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGunRemoveAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLkd1blJlbW92ZUFjay5wcm90bxIFdHlwZXMaFXR5cGUuRGF0YUNo" + "YW5nZS5wcm90bxoYdHlwZS5QbGF5ZXJHdW5JbmZvLnByb3RvIlwKDEd1blJl" + "bW92ZUFjaxIlCgdHdW5JbmZvGAEgASgLMhQudHlwZXMuUGxheWVyR3VuSW5m" + "bxIlCgpEYXRhQ2hhbmdlGAIgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePlayerGunInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GunRemoveAck), (MessageParser)(object)GunRemoveAck.Parser, new string[2] { "GunInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
