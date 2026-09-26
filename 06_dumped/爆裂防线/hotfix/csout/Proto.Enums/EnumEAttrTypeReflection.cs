using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEAttrTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEAttrTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVBdHRyVHlwZS5wcm90bxIJRUF0dHJUeXBlKkoKCUVBdHRyVHlw" + "ZRIOCgpFbXB0eVZhbHVlEAASCAoEVGVhbRABEgkKBVNraWxsEAISCwoHRGVm" + "ZW5zZRADEgsKB01vbnN0ZXIQBEIuWgkuLztwcm90b2OqAgtQcm90by5FbnVt" + "c8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EAttrType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
