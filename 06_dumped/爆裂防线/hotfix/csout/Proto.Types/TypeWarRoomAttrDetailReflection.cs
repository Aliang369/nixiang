using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWarRoomAttrDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWarRoomAttrDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLldhclJvb21BdHRyRGV0YWlsLnByb3RvEgV0eXBlcxoZZW51bS5F" + "QXR0clNraWxsVHlwZS5wcm90bxoedHlwZS5XYXJSb29tQXR0ckZyb21JbmZv" + "LnByb3RvIo4BChFXYXJSb29tQXR0ckRldGFpbBIsCgRUeXBlGAEgASgOMh4u" + "RUF0dHJTa2lsbFR5cGUuRUF0dHJTa2lsbFR5cGUSDQoFVmFsdWUYAiABKAIS" + "DgoGR3Jvd3RoGAMgASgCEiwKCEZyb21JbmZvGAQgASgLMhoudHlwZXMuV2Fy" + "Um9vbUF0dHJGcm9tSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEAttrSkillTypeReflection.Descriptor,
			TypeWarRoomAttrFromInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WarRoomAttrDetail), (MessageParser)(object)WarRoomAttrDetail.Parser, new string[4] { "Type", "Value", "Growth", "FromInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
