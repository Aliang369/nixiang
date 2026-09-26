using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEquipSlotGemReplaceReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotGemReplaceReqReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkVxdWlwU2xvdEdlbVJlcGxhY2VSZXEucHJvdG8SBXR5cGVzGhVl" + "bnVtLkVFcXVpcFR5cGUucHJvdG8idQoWRXF1aXBTbG90R2VtUmVwbGFjZVJl" + "cRIRCglHZW1QbGFuSWQYASABKAUSKQoJRXF1aXBUeXBlGAIgASgOMhYuRUVx" + "dWlwVHlwZS5FRXF1aXBUeXBlEg0KBUluZGV4GAMgASgFEg4KBkdlbVRpZBgE" + "IAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEEquipTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotGemReplaceReq), (MessageParser)(object)EquipSlotGemReplaceReq.Parser, new string[4] { "GemPlanId", "EquipType", "Index", "GemTid" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
