using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEPropTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEPropTypeReflection()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVQcm9wVHlwZS5wcm90bxIJRVByb3BUeXBlKs4CCglFUHJvcFR5" + "cGUSDgoKRW1wdHlWYWx1ZRAAEgwKCE1hdGVyaWFsEAESBwoDT3JlEAISCwoH" + "UGlja2F4ZRADEgkKBURyaWxsEAQSCAoEQm9tYhAFEggKBFN1aXQQCBIHCgNH" + "dW4QCRIICgRIZWFkEAoSDQoJSGVhZEZyYW1lEAsSCQoFTWVjaGEQDBIICgRX" + "YWxsEA0SDAoIRm9vdGhvbGQQDhIRCg1Wb3VjaGVyQ291cG9uEA8SCgoGV29y" + "a2VyEBASDwoLU3VpdENvbnZlcnQQERIOCgpHdW5Db252ZXJ0EBISEAoMTWVj" + "aGFDb252ZXJ0EBMSDwoLV2FsbENvbnZlcnQQFBITCg9Gb290aG9sZENvbnZl" + "cnQQFRIMCghUcmVhc3VyZRAWEg8KC1NlcnZhbnRFZ2dzEBcSEgoOU2VydmFu" + "dENvbnZlcnQQGEIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2Ft" + "ZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EPropType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
