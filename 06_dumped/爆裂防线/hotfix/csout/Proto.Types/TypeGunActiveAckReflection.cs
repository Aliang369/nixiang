using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeGunActiveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGunActiveAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLkd1bkFjdGl2ZUFjay5wcm90bxIFdHlwZXMaFXR5cGUuRGF0YUNo" + "YW5nZS5wcm90bxoYdHlwZS5QbGF5ZXJHdW5JbmZvLnByb3RvImsKDEd1bkFj" + "dGl2ZUFjaxINCgVHdW5JZBgBIAEoBRIlCgdHdW5JbmZvGAIgASgLMhQudHlw" + "ZXMuUGxheWVyR3VuSW5mbxIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMu" + "RGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePlayerGunInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GunActiveAck), (MessageParser)(object)GunActiveAck.Parser, new string[3] { "GunId", "GunInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
