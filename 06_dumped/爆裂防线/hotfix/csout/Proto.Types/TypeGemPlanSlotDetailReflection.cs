using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeGemPlanSlotDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGemPlanSlotDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkdlbVBsYW5TbG90RGV0YWlsLnByb3RvEgV0eXBlcxoVZW51bS5F" + "RXF1aXBUeXBlLnByb3RvGhh0eXBlLkdlbVNsb3REZXRhaWwucHJvdG8iaQoR" + "R2VtUGxhblNsb3REZXRhaWwSKQoJRXF1aXBUeXBlGAEgASgOMhYuRUVxdWlw" + "VHlwZS5FRXF1aXBUeXBlEikKC0dlbVNsb3RMaXN0GAIgAygLMhQudHlwZXMu" + "R2VtU2xvdERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEEquipTypeReflection.Descriptor,
			TypeGemSlotDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GemPlanSlotDetail), (MessageParser)(object)GemPlanSlotDetail.Parser, new string[2] { "EquipType", "GemSlotList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
