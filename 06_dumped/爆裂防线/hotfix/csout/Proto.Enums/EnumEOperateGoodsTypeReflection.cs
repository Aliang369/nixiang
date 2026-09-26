using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEOperateGoodsTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEOperateGoodsTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChxlbnVtLkVPcGVyYXRlR29vZHNUeXBlLnByb3RvEhFFT3BlcmF0ZUdvb2Rz" + "VHlwZSpcChFFT3BlcmF0ZUdvb2RzVHlwZRIOCgpFbXB0eVZhbHVlEAASDQoJ" + "TmV3U2VydmVyEAESDQoJT2xkU2VydmVyEAISCwoHSG9saWRheRADEgwKCElu" + "ZmluaXRlEARCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVc" + "UHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EOperateGoodsType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
