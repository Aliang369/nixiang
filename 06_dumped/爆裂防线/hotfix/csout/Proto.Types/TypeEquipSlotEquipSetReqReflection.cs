using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEquipSlotEquipSetReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotEquipSetReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkVxdWlwU2xvdEVxdWlwU2V0UmVxLnByb3RvEgV0eXBlcxoVZW51" + "bS5FRXF1aXBUeXBlLnByb3RvIlIKFEVxdWlwU2xvdEVxdWlwU2V0UmVxEikK" + "CUVxdWlwVHlwZRgBIAEoDjIWLkVFcXVpcFR5cGUuRUVxdWlwVHlwZRIPCgdF" + "cXVpcElkGAIgASgJQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEEquipTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotEquipSetReq), (MessageParser)(object)EquipSlotEquipSetReq.Parser, new string[2] { "EquipType", "EquipId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
