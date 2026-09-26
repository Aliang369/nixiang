using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEquipSlotDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkVxdWlwU2xvdERldGFpbC5wcm90bxIFdHlwZXMaFWVudW0uRUVx" + "dWlwVHlwZS5wcm90bxoWdHlwZS5FcXVpcERldGFpbC5wcm90byJuCg9FcXVp" + "cFNsb3REZXRhaWwSKQoJRXF1aXBUeXBlGAEgASgOMhYuRUVxdWlwVHlwZS5F" + "RXF1aXBUeXBlEg0KBUxldmVsGAIgASgFEiEKBUVxdWlwGAMgASgLMhIudHlw" + "ZXMuRXF1aXBEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEEquipTypeReflection.Descriptor,
			TypeEquipDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotDetail), (MessageParser)(object)EquipSlotDetail.Parser, new string[3] { "EquipType", "Level", "Equip" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
