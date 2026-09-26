using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeVoucherRecentBuyDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherRecentBuyDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlZvdWNoZXJSZWNlbnRCdXlEZXRhaWwucHJvdG8SBXR5cGVzGhVl" + "bnVtLkVCb29sU3RhdGUucHJvdG8iVAoWVm91Y2hlclJlY2VudEJ1eURldGFp" + "bBIPCgdHb29kc0lkGAEgASgFEikKCVNhbGVTdGF0ZRgCIAEoDjIWLkVCb29s" + "U3RhdGUuRUJvb2xTdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherRecentBuyDetail), (MessageParser)(object)VoucherRecentBuyDetail.Parser, new string[2] { "GoodsId", "SaleState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
