using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEquipSlotGemSetReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotGemSetReqReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkVxdWlwU2xvdEdlbVNldFJlcS5wcm90bxIFdHlwZXMaFWVudW0u" + "RUVxdWlwVHlwZS5wcm90byJiChJFcXVpcFNsb3RHZW1TZXRSZXESEQoJR2Vt" + "UGxhbklkGAEgASgFEikKCUVxdWlwVHlwZRgCIAEoDjIWLkVFcXVpcFR5cGUu" + "RUVxdWlwVHlwZRIOCgZHZW1UaWQYAyABKAVCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEEquipTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotGemSetReq), (MessageParser)(object)EquipSlotGemSetReq.Parser, new string[3] { "GemPlanId", "EquipType", "GemTid" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
