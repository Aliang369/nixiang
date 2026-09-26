using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementLimitTimeRouletteAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementLimitTimeRouletteAckReflection()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cix0eXBlLkFkdmVydGlzZW1lbnRMaW1pdFRpbWVSb3VsZXR0ZUFjay5wcm90" + "bxIFdHlwZXMaHHR5cGUuQWR2ZXJ0aXNlbWVudEluZm8ucHJvdG8aFnR5cGUu" + "QXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoidHlw" + "ZS5MaW1pdFRpbWVSb3VsZXR0ZURldGFpbC5wcm90byLlAQohQWR2ZXJ0aXNl" + "bWVudExpbWl0VGltZVJvdWxldHRlQWNrEjMKEUFkdmVydGlzZW1lbnRJbmZv" + "GAEgASgLMhgudHlwZXMuQWR2ZXJ0aXNlbWVudEluZm8SPQoVTGltaXRUaW1l" + "Um91bGV0dGVMaXN0GAIgAygLMh4udHlwZXMuTGltaXRUaW1lUm91bGV0dGVE" + "ZXRhaWwSJQoJQXdhcmRMaXN0GAMgAygLMhIudHlwZXMuQXdhcmREZXRhaWwS" + "JQoKRGF0YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLimitTimeRouletteDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementLimitTimeRouletteAck), (MessageParser)(object)AdvertisementLimitTimeRouletteAck.Parser, new string[4] { "AdvertisementInfo", "LimitTimeRouletteList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
