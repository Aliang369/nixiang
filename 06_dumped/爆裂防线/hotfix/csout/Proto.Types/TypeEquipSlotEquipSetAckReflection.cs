using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEquipSlotEquipSetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotEquipSetAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkVxdWlwU2xvdEVxdWlwU2V0QWNrLnByb3RvEgV0eXBlcxoVdHlw" + "ZS5EYXRhQ2hhbmdlLnByb3RvGhh0eXBlLkVxdWlwU2xvdEluZm8ucHJvdG8i" + "agoURXF1aXBTbG90RXF1aXBTZXRBY2sSKwoNRXF1aXBTbG90SW5mbxgBIAEo" + "CzIULnR5cGVzLkVxdWlwU2xvdEluZm8SJQoKRGF0YUNoYW5nZRgCIAEoCzIR" + "LnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeEquipSlotInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotEquipSetAck), (MessageParser)(object)EquipSlotEquipSetAck.Parser, new string[2] { "EquipSlotInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
