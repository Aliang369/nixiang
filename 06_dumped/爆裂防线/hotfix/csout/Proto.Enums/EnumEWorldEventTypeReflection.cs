using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEWorldEventTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEWorldEventTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChplbnVtLkVXb3JsZEV2ZW50VHlwZS5wcm90bxIPRVdvcmxkRXZlbnRUeXBl" + "KlYKD0VXb3JsZEV2ZW50VHlwZRIOCgpFbXB0eVZhbHVlEAASDAoIQXdhcmRC" + "b3gQARIJCgVGaWdodBACEgsKB0R1bmdlb24QAxINCglXb3JsZExhaXIQBEIu" + "WgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVt" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EWorldEventType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
