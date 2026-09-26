using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWarRoomAttrFromInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWarRoomAttrFromInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLldhclJvb21BdHRyRnJvbUluZm8ucHJvdG8SBXR5cGVzIogBChNX" + "YXJSb29tQXR0ckZyb21JbmZvEhEKCVNraWxsQmFzZRgBIAEoAhIPCgdXYXJS" + "b29tGAIgASgCEhEKCUVxdWlwU2xvdBgDIAEoAhIMCgRIZXJvGAQgASgCEgsK" + "A0d1bhgFIAEoAhIPCgdTZXJ2YW50GAYgASgCEg4KBlRhY3RpYxgHIAEoAkIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WarRoomAttrFromInfo), (MessageParser)(object)WarRoomAttrFromInfo.Parser, new string[7] { "SkillBase", "WarRoom", "EquipSlot", "Hero", "Gun", "Servant", "Tactic" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
