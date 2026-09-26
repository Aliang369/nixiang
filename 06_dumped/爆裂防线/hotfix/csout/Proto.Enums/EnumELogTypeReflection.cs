using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumELogTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumELogTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChNlbnVtLkVMb2dUeXBlLnByb3RvEghFTG9nVHlwZSo6CghFTG9nVHlwZRIO" + "CgpFbXB0eVZhbHVlEAASCgoGTGVnaW9uEAESEgoOQ3Jvc3NHcmFkZVJhY2UQ" + "AkIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xF" + "bnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ELogType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
