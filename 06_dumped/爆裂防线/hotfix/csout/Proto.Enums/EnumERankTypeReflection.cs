using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumERankTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumERankTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVSYW5rVHlwZS5wcm90bxIJRVJhbmtUeXBlKmUKCUVSYW5rVHlw" + "ZRIOCgpFbXB0eVZhbHVlEAASDQoJV29ybGRCb3NzEAESDQoJTWFpblF1ZXN0" + "EAISEAoMRW5kbGVzc0hvcmRlEAMSCQoFQXJlbmEQBBINCglQZWFrQXJlbmEQ" + "BUIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xF" + "bnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ERankType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
