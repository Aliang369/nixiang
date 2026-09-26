using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEPlatformTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEPlatformTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChhlbnVtLkVQbGF0Zm9ybVR5cGUucHJvdG8SDUVQbGF0Zm9ybVR5cGUqeAoN" + "RVBsYXRmb3JtVHlwZRIOCgpFbXB0eVZhbHVlEAASBwoDRGV2EAESCAoEVGVz" + "dBACEgcKA0V2YRADEggKBERlbW8QBBIJCgVCZXRhMRAFEgkKBUJldGEyEAYS" + "CQoFQmV0YTMQBxIHCgNHYTEQCxIHCgNHYTIQDEIuWgkuLztwcm90b2OqAgtQ" + "cm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EPlatformType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
