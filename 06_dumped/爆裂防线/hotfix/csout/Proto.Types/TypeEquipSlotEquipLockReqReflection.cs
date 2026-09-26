using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEquipSlotEquipLockReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotEquipLockReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkVxdWlwU2xvdEVxdWlwTG9ja1JlcS5wcm90bxIFdHlwZXMaFWVu" + "dW0uRUJvb2xTdGF0ZS5wcm90bxoVZW51bS5FRXF1aXBUeXBlLnByb3RvImgK" + "FUVxdWlwU2xvdEVxdWlwTG9ja1JlcRIpCglFcXVpcFR5cGUYASABKA4yFi5F" + "RXF1aXBUeXBlLkVFcXVpcFR5cGUSJAoETG9jaxgCIAEoDjIWLkVCb29sU3Rh" + "dGUuRUJvb2xTdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumEEquipTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotEquipLockReq), (MessageParser)(object)EquipSlotEquipLockReq.Parser, new string[2] { "EquipType", "Lock" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
