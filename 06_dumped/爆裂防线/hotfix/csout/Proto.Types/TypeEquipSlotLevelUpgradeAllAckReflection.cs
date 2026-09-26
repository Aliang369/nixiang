using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEquipSlotLevelUpgradeAllAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotLevelUpgradeAllAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLkVxdWlwU2xvdExldmVsVXBncmFkZUFsbEFjay5wcm90bxIFdHlw" + "ZXMaFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoYdHlwZS5FcXVpcFNsb3RJbmZv" + "LnByb3RvInEKG0VxdWlwU2xvdExldmVsVXBncmFkZUFsbEFjaxIrCg1FcXVp" + "cFNsb3RJbmZvGAEgASgLMhQudHlwZXMuRXF1aXBTbG90SW5mbxIlCgpEYXRh" + "Q2hhbmdlGAIgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeEquipSlotInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotLevelUpgradeAllAck), (MessageParser)(object)EquipSlotLevelUpgradeAllAck.Parser, new string[2] { "EquipSlotInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
