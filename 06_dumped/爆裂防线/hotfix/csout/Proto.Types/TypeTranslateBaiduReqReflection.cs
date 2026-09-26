using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeTranslateBaiduReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTranslateBaiduReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlRyYW5zbGF0ZUJhaWR1UmVxLnByb3RvEgV0eXBlcxoZZW51bS5F" + "VHJhbnNsYXRlVHlwZS5wcm90byJ8ChFUcmFuc2xhdGVCYWlkdVJlcRINCgVR" + "dWVyeRgBIAEoCRIsCgRGcm9tGAIgASgOMh4uRVRyYW5zbGF0ZVR5cGUuRVRy" + "YW5zbGF0ZVR5cGUSKgoCVG8YAyABKA4yHi5FVHJhbnNsYXRlVHlwZS5FVHJh" + "bnNsYXRlVHlwZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumETranslateTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TranslateBaiduReq), (MessageParser)(object)TranslateBaiduReq.Parser, new string[3] { "Query", "From", "To" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
