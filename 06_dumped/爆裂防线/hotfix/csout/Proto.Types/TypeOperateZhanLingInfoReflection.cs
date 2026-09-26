using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOperateZhanLingInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateZhanLingInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLk9wZXJhdGVaaGFuTGluZ0luZm8ucHJvdG8SBXR5cGVzGhVlbnVt" + "LkVCb29sU3RhdGUucHJvdG8ikQEKE09wZXJhdGVaaGFuTGluZ0luZm8SDwoH" + "R29vZHNJZBgBIAEoBRINCgVMZXZlbBgCIAEoBRINCgVSZWFjaBgDIAEoBRIm" + "CgZBY3RpdmUYBCABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGUSEAoITG93" + "QXdhcmQYBSABKAUSEQoJSGlnaEF3YXJkGAYgASgFQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateZhanLingInfo), (MessageParser)(object)OperateZhanLingInfo.Parser, new string[6] { "GoodsId", "Level", "Reach", "Active", "LowAward", "HighAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
