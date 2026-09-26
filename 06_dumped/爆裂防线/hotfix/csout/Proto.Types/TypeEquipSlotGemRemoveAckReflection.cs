using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEquipSlotGemRemoveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotGemRemoveAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkVxdWlwU2xvdEdlbVJlbW92ZUFjay5wcm90bxIFdHlwZXMaFXR5" + "cGUuRGF0YUNoYW5nZS5wcm90bxoYdHlwZS5FcXVpcFNsb3RJbmZvLnByb3Rv" + "ImsKFUVxdWlwU2xvdEdlbVJlbW92ZUFjaxIrCg1FcXVpcFNsb3RJbmZvGAEg" + "ASgLMhQudHlwZXMuRXF1aXBTbG90SW5mbxIlCgpEYXRhQ2hhbmdlGAIgASgL" + "MhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeEquipSlotInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotGemRemoveAck), (MessageParser)(object)EquipSlotGemRemoveAck.Parser, new string[2] { "EquipSlotInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
