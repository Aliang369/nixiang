using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleTipsEquipInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleTipsEquipInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlJvbGVUaXBzRXF1aXBJbmZvLnByb3RvEgV0eXBlcxoadHlwZS5F" + "cXVpcFNsb3REZXRhaWwucHJvdG8aHHR5cGUuR2VtUGxhblNsb3REZXRhaWwu" + "cHJvdG8icQoRUm9sZVRpcHNFcXVpcEluZm8SLQoNRXF1aXBTbG90TGlzdBgB" + "IAMoCzIWLnR5cGVzLkVxdWlwU2xvdERldGFpbBItCgtHZW1TbG90TGlzdBgC" + "IAMoCzIYLnR5cGVzLkdlbVBsYW5TbG90RGV0YWlsQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeEquipSlotDetailReflection.Descriptor,
			TypeGemPlanSlotDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleTipsEquipInfo), (MessageParser)(object)RoleTipsEquipInfo.Parser, new string[2] { "EquipSlotList", "GemSlotList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
