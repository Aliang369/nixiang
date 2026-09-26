using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEVoucherCouponTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEVoucherCouponTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch1lbnVtLkVWb3VjaGVyQ291cG9uVHlwZS5wcm90bxISRVZvdWNoZXJDb3Vw" + "b25UeXBlKkEKEkVWb3VjaGVyQ291cG9uVHlwZRIOCgpFbXB0eVZhbHVlEAAS" + "DQoJRGVkdWN0aW9uEAESDAoIRGlzY291bnQQAkIuWgkuLztwcm90b2OqAgtQ" + "cm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EVoucherCouponType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
