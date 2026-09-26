using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumENonOperatingTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumENonOperatingTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChxlbnVtLkVOb25PcGVyYXRpbmdUeXBlLnByb3RvEhFFTm9uT3BlcmF0aW5n" + "VHlwZSpHChFFTm9uT3BlcmF0aW5nVHlwZRIOCgpFbXB0eVZhbHVlEAASEwoP" + "UHJlUmVnaXN0cmF0aW9uEAESDQoJUmVwYXltZW50EAJCLloJLi87cHJvdG9j" + "qgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ENonOperatingType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
