using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSnowVeilTurntableSlotDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSnowVeilTurntableSlotDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLlNub3dWZWlsVHVybnRhYmxlU2xvdERldGFpbC5wcm90bxIFdHlw" + "ZXMaFWVudW0uRUJvb2xTdGF0ZS5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5w" + "cm90byJ2ChtTbm93VmVpbFR1cm50YWJsZVNsb3REZXRhaWwSDQoFSW5kZXgY" + "ASABKAUSJQoFU3RhdGUYAiABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGUS" + "IQoFQXdhcmQYAyABKAsyEi50eXBlcy5Bd2FyZERldGFpbEIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SnowVeilTurntableSlotDetail), (MessageParser)(object)SnowVeilTurntableSlotDetail.Parser, new string[3] { "Index", "State", "Award" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
