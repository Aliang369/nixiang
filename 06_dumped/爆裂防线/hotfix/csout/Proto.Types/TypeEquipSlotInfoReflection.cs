using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEquipSlotInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkVxdWlwU2xvdEluZm8ucHJvdG8SBXR5cGVzGhp0eXBlLkVxdWlw" + "U2xvdERldGFpbC5wcm90bxoYdHlwZS5HZW1QbGFuRGV0YWlsLnByb3RvInwK" + "DUVxdWlwU2xvdEluZm8SLQoNRXF1aXBTbG90TGlzdBgBIAMoCzIWLnR5cGVz" + "LkVxdWlwU2xvdERldGFpbBIpCgtHZW1QbGFuTGlzdBgCIAMoCzIULnR5cGVz" + "LkdlbVBsYW5EZXRhaWwSEQoJR2VtUGxhbklkGAMgASgFQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeEquipSlotDetailReflection.Descriptor,
			TypeGemPlanDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotInfo), (MessageParser)(object)EquipSlotInfo.Parser, new string[3] { "EquipSlotList", "GemPlanList", "GemPlanId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
