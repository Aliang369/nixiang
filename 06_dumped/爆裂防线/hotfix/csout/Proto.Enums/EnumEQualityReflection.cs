using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEQualityReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEQualityReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChNlbnVtLkVRdWFsaXR5LnByb3RvEghFUXVhbGl0eSpnCghFUXVhbGl0eRIO" + "CgpFbXB0eVZhbHVlEAASCQoFV2hpdGUQARIJCgVHcmVlbhACEggKBEJsdWUQ" + "AxIKCgZQdXJwbGUQBBIKCgZHb2xkZW4QBRIHCgNSZWQQBhIKCgZPcmFuZ2UQ" + "B0IuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xF" + "bnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EQuality) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
