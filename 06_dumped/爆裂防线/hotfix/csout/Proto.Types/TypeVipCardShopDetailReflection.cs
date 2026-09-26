using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeVipCardShopDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVipCardShopDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlZpcENhcmRTaG9wRGV0YWlsLnByb3RvEgV0eXBlcxoXZW51bS5F" + "QWN0aXZlU3RhdGUucHJvdG8aE2VudW0uRUNDWVR5cGUucHJvdG8ipwEKEVZp" + "cENhcmRTaG9wRGV0YWlsEg8KB0dvb2RzSWQYASABKAUSHwoDQ2N5GAIgASgO" + "MhIuRUNDWVR5cGUuRUNDWVR5cGUSDQoFUHJpY2UYAyABKAISDwoHRGlhbW9u" + "ZBgEIAEoBRIQCghFeHBpcmVUbRgFIAEoAxIuCgpEYWlseUF3YXJkGAYgASgO" + "MhouRUFjdGl2ZVN0YXRlLkVBY3RpdmVTdGF0ZUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEActiveStateReflection.Descriptor,
			EnumECCYTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VipCardShopDetail), (MessageParser)(object)VipCardShopDetail.Parser, new string[6] { "GoodsId", "Ccy", "Price", "Diamond", "ExpireTm", "DailyAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
