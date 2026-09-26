using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWarRoomUpgradeFastAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWarRoomUpgradeFastAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLldhclJvb21VcGdyYWRlRmFzdEFjay5wcm90bxIFdHlwZXMaFXR5" + "cGUuRGF0YUNoYW5nZS5wcm90bxoXdHlwZS5Ib21lbGFuZEluZm8ucHJvdG8a" + "FnR5cGUuV2FyUm9vbUluZm8ucHJvdG8ikgEKFVdhclJvb21VcGdyYWRlRmFz" + "dEFjaxIpCgxIb21lbGFuZEluZm8YASABKAsyEy50eXBlcy5Ib21lbGFuZElu" + "Zm8SJwoLV2FyUm9vbUluZm8YAiABKAsyEi50eXBlcy5XYXJSb29tSW5mbxIl" + "CgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDataChangeReflection.Descriptor,
			TypeHomelandInfoReflection.Descriptor,
			TypeWarRoomInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WarRoomUpgradeFastAck), (MessageParser)(object)WarRoomUpgradeFastAck.Parser, new string[3] { "HomelandInfo", "WarRoomInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
