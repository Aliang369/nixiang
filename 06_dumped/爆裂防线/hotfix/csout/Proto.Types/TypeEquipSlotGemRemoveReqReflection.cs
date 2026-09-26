using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEquipSlotGemRemoveReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotGemRemoveReqReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkVxdWlwU2xvdEdlbVJlbW92ZVJlcS5wcm90bxIFdHlwZXMaFWVu" + "dW0uRUVxdWlwVHlwZS5wcm90byJkChVFcXVpcFNsb3RHZW1SZW1vdmVSZXES" + "EQoJR2VtUGxhbklkGAEgASgFEikKCUVxdWlwVHlwZRgCIAEoDjIWLkVFcXVp" + "cFR5cGUuRUVxdWlwVHlwZRINCgVJbmRleBgDIAEoBUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEEquipTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotGemRemoveReq), (MessageParser)(object)EquipSlotGemRemoveReq.Parser, new string[3] { "GemPlanId", "EquipType", "Index" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
