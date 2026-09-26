using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarWatchMapInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarWatchMapInfoAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkxlZ2lvbldhcldhdGNoTWFwSW5mb0Fjay5wcm90bxIFdHlwZXMa" + "G3R5cGUuTGVnaW9uV2FyTWFwSW5mby5wcm90byJNChhMZWdpb25XYXJXYXRj" + "aE1hcEluZm9BY2sSMQoQTGVnaW9uV2FyTWFwSW5mbxgBIAEoCzIXLnR5cGVz" + "LkxlZ2lvbldhck1hcEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeLegionWarMapInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarWatchMapInfoAck), (MessageParser)(object)LegionWarWatchMapInfoAck.Parser, new string[1] { "LegionWarMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
