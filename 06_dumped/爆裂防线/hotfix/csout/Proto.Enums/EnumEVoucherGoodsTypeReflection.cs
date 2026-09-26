using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEVoucherGoodsTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEVoucherGoodsTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChxlbnVtLkVWb3VjaGVyR29vZHNUeXBlLnByb3RvEhFFVm91Y2hlckdvb2Rz" + "VHlwZSo6ChFFVm91Y2hlckdvb2RzVHlwZRIOCgpFbXB0eVZhbHVlEAASCgoG" + "Q29tbW9uEAESCQoFTGltaXQQAkIuWgkuLztwcm90b2OqAgtQcm90by5FbnVt" + "c8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EVoucherGoodsType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
