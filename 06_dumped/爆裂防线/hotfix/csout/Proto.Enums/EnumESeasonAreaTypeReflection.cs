using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonAreaTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonAreaTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChplbnVtLkVTZWFzb25BcmVhVHlwZS5wcm90bxIPRVNlYXNvbkFyZWFUeXBl" + "KjUKD0VTZWFzb25BcmVhVHlwZRIJCgVCaXJ0aBAAEgsKB05ldXRyYWwQARIK" + "CgZDZW50ZXIQAkIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2Ft" + "ZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonAreaType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
