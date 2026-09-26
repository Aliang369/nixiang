using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeNovicePackInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeNovicePackInfoReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLk5vdmljZVBhY2tJbmZvLnByb3RvEgV0eXBlcxoXZW51bS5FQWN0" + "aXZlU3RhdGUucHJvdG8aFWVudW0uRUJvb2xTdGF0ZS5wcm90bxogdHlwZS5S" + "ZWNoYXJnZUdvb2RzQmFzZUluZm8ucHJvdG8iugEKDk5vdmljZVBhY2tJbmZv" + "EjMKDUdvb2RzQmFzZUluZm8YASABKAsyHC50eXBlcy5SZWNoYXJnZUdvb2Rz" + "QmFzZUluZm8SJQoFU3RhdGUYAiABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3Rh" + "dGUSLgoKVG9kYXlTdGF0ZRgDIAEoDjIaLkVBY3RpdmVTdGF0ZS5FQWN0aXZl" + "U3RhdGUSDQoFQ291bnQYBCABKAUSDQoFVG90YWwYBSABKAVCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEActiveStateReflection.Descriptor,
			EnumEBoolStateReflection.Descriptor,
			TypeRechargeGoodsBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(NovicePackInfo), (MessageParser)(object)NovicePackInfo.Parser, new string[5] { "GoodsBaseInfo", "State", "TodayState", "Count", "Total" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
