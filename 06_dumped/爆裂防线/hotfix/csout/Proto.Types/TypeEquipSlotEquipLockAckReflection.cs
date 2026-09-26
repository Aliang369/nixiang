using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEquipSlotEquipLockAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotEquipLockAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkVxdWlwU2xvdEVxdWlwTG9ja0Fjay5wcm90bxIFdHlwZXMaGHR5" + "cGUuRXF1aXBTbG90SW5mby5wcm90byJEChVFcXVpcFNsb3RFcXVpcExvY2tB" + "Y2sSKwoNRXF1aXBTbG90SW5mbxgBIAEoCzIULnR5cGVzLkVxdWlwU2xvdElu" + "Zm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeEquipSlotInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotEquipLockAck), (MessageParser)(object)EquipSlotEquipLockAck.Parser, new string[1] { "EquipSlotInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
