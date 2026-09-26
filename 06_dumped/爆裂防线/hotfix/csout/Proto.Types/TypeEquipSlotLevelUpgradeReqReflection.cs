using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEquipSlotLevelUpgradeReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotLevelUpgradeReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkVxdWlwU2xvdExldmVsVXBncmFkZVJlcS5wcm90bxIFdHlwZXMa" + "FWVudW0uRUVxdWlwVHlwZS5wcm90byJFChhFcXVpcFNsb3RMZXZlbFVwZ3Jh" + "ZGVSZXESKQoJRXF1aXBUeXBlGAEgASgOMhYuRUVxdWlwVHlwZS5FRXF1aXBU" + "eXBlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEEquipTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotLevelUpgradeReq), (MessageParser)(object)EquipSlotLevelUpgradeReq.Parser, new string[1] { "EquipType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
