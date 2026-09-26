using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZlbnVtLkVTZWFzb25UeXBlLnByb3RvEgtFU2Vhc29uVHlwZSoxCgtFU2Vh" + "c29uVHlwZRIOCgpFbXB0eVZhbHVlEAASCgoGTmV3YmllEAESBgoCUEsQAkIu" + "WgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVt" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
